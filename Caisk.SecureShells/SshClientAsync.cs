using Renci.SshNet.Common;

namespace Caisk.SecureShells;

[PublicAPI]
public class SshClientAsync : IAsyncDisposable
{
    private readonly BlockingCollection<SshAction> _queue = new(new ConcurrentQueue<SshAction>());
    private readonly SshClient _sshClient;
    private CancellationTokenSource? _cancellationSource;
    private Task? _thread;

    public SshClientAsync(SshClient sshClient)
    {
        _sshClient = sshClient;
    }

    public SshClientAsync(ConnectionInfo connectionInfo)
    {
        _sshClient = new SshClient(connectionInfo);
        _sshClient.HostKeyReceived += HostKeyReceived;
    }

    public string? FingerPrint { get; private set; }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }

    private void HostKeyReceived(object? sender, HostKeyEventArgs e)
    {
        FingerPrint = Convert.ToHexString(e.FingerPrint);
        e.CanTrust = true;
    }

    private void ThreadProc(SshClient ssh, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var action = _queue.Take(cancellationToken);
                action.Process(_sshClient, cancellationToken);
                if (action is SshDisconnectAction)
                    break;
            }
        }
        catch (TaskCanceledException)
        {
        }
        finally
        {
            ssh.Dispose();
        }
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_sshClient.IsConnected)
            throw new Exception("Connection is already established");

        _cancellationSource = new CancellationTokenSource();
        _thread = Task.Run(() => ThreadProc(_sshClient, _cancellationSource.Token));

        var action = new SshConnectAction(cancellationToken);
        _queue.Add(action, cancellationToken);
        await action.Task;
    }

    public async Task DisconnectAsync()
    {
        var action = new SshDisconnectAction(default);
        _queue.Add(action);
        await action.Task;

        if (_thread != null)
            await _thread;
        _thread = null;

        _queue.CompleteAdding();
        // Ask thread to shutdown
        _cancellationSource?.Cancel();
    }

    public async Task<CommandResult> SendCommandAsync(string commandText, CancellationToken cancellationToken = default)
    {
        var action = new SshCommandAction(commandText, cancellationToken);
        _queue.Add(action, cancellationToken);
        await action.Task;
        return action.Result;
    }

    public async Task<string> GetStringAsync(string command, CancellationToken cancellationToken = default)
    {
        var result = await SendCommandAsync(command, cancellationToken);
        if (result.ExitCode != 0 || !string.IsNullOrWhiteSpace(result.Error))
            throw new Exception($"Exit code {result.ExitCode}: {result.Error}");
        return result.Result;
    }

    public async Task ExecuteAsync(string command, CancellationToken cancellationToken = default)
    {
        var result = await SendCommandAsync(command, cancellationToken);
        if (result.ExitCode != 0)
            throw new Exception($"Exit code {result.ExitCode}: {result.Error}");
    }

    public async Task<CommandResult> SendStreamAsync(Func<ShellStream, CancellationToken, CommandResult> callback,
        CancellationToken cancellationToken = default)
    {
        var action = new SshStreamAction(callback, cancellationToken);
        _queue.Add(action, cancellationToken);
        await action.Task;
        return action.Result;
    }

    private abstract class SshAction
    {
        private readonly CancellationToken _cancellationToken;
        private readonly TaskCompletionSource _taskCompletionSource = new();

        protected SshAction(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public Task Task => _taskCompletionSource.Task;

        public void Process(SshClient ssh, CancellationToken cancellationToken)
        {
            var token = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationToken).Token;
            try
            {
                Run(ssh, token);
                _taskCompletionSource.SetResult();
            }
            catch (TaskCanceledException)
            {
                _taskCompletionSource.SetCanceled(cancellationToken);
            }
            catch (Exception ex)
            {
                _taskCompletionSource.SetException(ex);
            }
        }

        protected abstract void Run(SshClient ssh, CancellationToken cancellationToken);
    }

    private class SshConnectAction : SshAction
    {
        public SshConnectAction(CancellationToken cancellationToken) : base(cancellationToken)
        {
        }

        protected override void Run(SshClient ssh, CancellationToken cancellationToken)
        {
            ssh.Connect();
        }
    }

    private class SshDisconnectAction : SshAction
    {
        public SshDisconnectAction(CancellationToken cancellationToken) : base(cancellationToken)
        {
        }

        protected override void Run(SshClient ssh, CancellationToken cancellationToken)
        {
            ssh.Disconnect();
        }
    }

    private class SshStreamAction : SshAction
    {
        private readonly Func<ShellStream, CancellationToken, CommandResult> _action;

        public SshStreamAction(Func<ShellStream, CancellationToken, CommandResult> action,
            CancellationToken cancellationToken) : base(cancellationToken)
        {
            _action = action;
        }

        public CommandResult Result { get; private set; } = null!;

        protected override void Run(SshClient ssh, CancellationToken cancellationToken)
        {
            using var shellStream = ssh.CreateShellStream("xterm", 80, 25, 800, 600, 1024);
            Result = _action.Invoke(shellStream, cancellationToken);
        }
    }

    private class SshCommandAction : SshAction
    {
        private readonly string _action;

        public SshCommandAction(string action, CancellationToken cancellationToken) : base(cancellationToken)
        {
            _action = action;
        }

        public CommandResult Result { get; private set; } = null!;

        protected override void Run(SshClient ssh, CancellationToken cancellationToken)
        {
            var command = ssh.CreateCommand(_action);
            var registration = cancellationToken.Register(() => command.CancelAsync());
            try
            {
                command.Execute();
                Result = new CommandResult(command);
            }
            finally
            {
                registration.Dispose();
            }
        }
    }
}

[PublicAPI]
public record CommandResult(int ExitCode, string Result, string Error)
{
    public CommandResult(SshCommand command) : this(command.ExitStatus, command.Result, command.Error)
    {
    }
};