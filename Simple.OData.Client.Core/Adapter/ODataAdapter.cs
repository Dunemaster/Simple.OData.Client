﻿using System;
using System.Collections.Generic;

namespace Simple.OData.Client
{
    abstract class ODataAdapter
    {
        public abstract AdapterVersion AdapterVersion { get; }
        public string ProtocolVersion { get; set; }
        public object Model { get; set; }

        public abstract IMetadata GetMetadata();
        public abstract IResponseReader GetResponseReader();
        public abstract IRequestWriter GetRequestWriter(Lazy<IBatchWriter> deferredBatchWriter);
        public abstract IBatchWriter GetBatchWriter();
    }
}