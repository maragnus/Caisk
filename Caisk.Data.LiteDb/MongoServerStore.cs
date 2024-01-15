﻿using Caisk.Managers.Mongo;

namespace Caisk.Data.LiteDb;

internal class MongoServerStore(ILiteCollection<MongoServerProfile> collection)
    : BaseStore<MongoServerProfile>(collection), IMongoServerStore;