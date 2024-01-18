# Caisk
_Container Apps In Simplified Kubernetes_

This is a web application and set of tools designed for configuring and maintaining containerized applications on Docker servers or a Kubernetes (K8S) cluster for web administrators accustomed to running LAMP-style servers.

Simplicity of LAMP on Docker and Kubernetes.

This installs and manages:
* **Ingress** and **Load Balancing** with [Traefik](https://traefik.io/traefik/)
* **HTTPS SSL** with [Let's Encrypt](https://letsencrypt.org/) and [cert-manager](https://cert-manager.io/)
* Secure deployments of **databases**
  * [MongoDB Community Edition](https://www.mongodb.com/)
  * [PostgreSQL](https://www.postgresql.org/)
* **Secret Management** handled internally

## Applications
An **Application** represents a single application and manages respective Kubernetes Objects. And **Application Group** is a collection of Applications that can share **Managed Secrets** and other resources between applications in the same group.

Applications are configured in a simplified way in Caisk.
* Name
* Container
* Expose ports
* Environment variables
* Registry credentials
* Replica count
* Volumes
* Hostnames
* External ports

The following Kubernetes Objects are managed at the Application level
* Deployment
* Service
* Secret
* Ingress
* Persistent Volume

## Current Support
* Add, edit, deploy, and stop docker-compose services
* Configure services for SSH, Mongo Database, Docker Registry

* **NOTE:** Kubernetes is not supported yet.

## Multi-tenancy
While this application is designed to host multiple unrelated applications, it does not currently provide a principle of least privilege with individual access permissions.

* **Kubernetes** is accessed directly using local `kubectl` or via SSH with namespace: `ApplicationName-EnvironmentName` 
* **Docker Compose** files are stored on the Secure Shell account: `~/.caisk/ApplicationName/EnvironmentName`

## Managed Secrets
Databases are managed by Caisk. Databases and users are created by Caisk and stored as K8S Secrets. These can be linked to applications via configuration files or environment variables.
Other secrets (such as API keys) can be shared with multiple applications or application groups.

* **Kubernetes** stores as Secrets
* **Docker Compose** stores in environment files on the filesystem

# Usage and License
This uses the GPL v3 license with some additional clauses and clarifications:
* This application is free to use and deploy as is, even for commercial purposes.
* The application must always reference the original application name of "Caisk" in a prominent area.
* All changes to the application source must be open-sourced and easily discoverable.
* No liability on authors or maintainers of this repository is assumed for use of this software.

This means that even if you need to customize the implementation for your particular one-off scenario, you MUST share those changes and link back to this original repository. You are free to integrate this into commercial services, any changes to the source code to support this must be shared and linked via GitHub to this repository. Rebranding is acceptable, but in all locations with the new name, it must visually mention "Caisk". Phrases such as "Powered by Caisk", "Fork of Caisk", and "Built on Caisk" are sufficient.
