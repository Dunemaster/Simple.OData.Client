﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Spatial;
using System.Xml;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using Simple.OData.Client.Extensions;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public static class V3Adapter
    {
        public static void Reference() { }
    }
}

#pragma warning disable 1591

namespace Simple.OData.Client.V3.Adapter
{
    public class ODataAdapter : ODataAdapterBase
    {
        private readonly ISession _session;

        public override AdapterVersion AdapterVersion { get { return AdapterVersion.V3; } }

        public override ODataPayloadFormat DefaultPayloadFormat
        {
            get { return ODataPayloadFormat.Atom; }
        }

        public new IEdmModel Model
        {
            get { return base.Model as IEdmModel; }
            set { base.Model = value; }
        }

        private ODataAdapter(ISession session, string protocolVersion)
        {
            _session = session;
            ProtocolVersion = protocolVersion;

            CustomConverters.RegisterTypeConverter(typeof(GeographyPoint), TypeConverters.CreateGeographyPoint);
            CustomConverters.RegisterTypeConverter(typeof(GeometryPoint), TypeConverters.CreateGeometryPoint);
        }

        public ODataAdapter(ISession session, string protocolVersion, HttpResponseMessage response)
            : this(session, protocolVersion)
        {
            var readerSettings = new ODataMessageReaderSettings
            {
                MessageQuotas = { MaxReceivedMessageSize = Int32.MaxValue }
            };
            using (var messageReader = new ODataMessageReader(new ODataResponseMessage(response), readerSettings))
            {
                Model = messageReader.ReadMetadataDocument();
            }
        }

        public ODataAdapter(ISession session, string protocolVersion, string metadataString)
            : this(session, protocolVersion)
        {
            var reader = XmlReader.Create(new StringReader(metadataString));
            reader.MoveToContent();
            Model = EdmxReader.Parse(reader);
        }

        public override string GetODataVersionString()
        {
            switch (this.ProtocolVersion)
            {
                case ODataProtocolVersion.V1:
                    return "V1";
                case ODataProtocolVersion.V2:
                    return "V2";
                case ODataProtocolVersion.V3:
                    return "V3";
            }
            throw new InvalidOperationException(string.Format("Unsupported OData protocol version: \"{0}\"", this.ProtocolVersion));
        }

        public override IMetadata GetMetadata()
        {
            return new Metadata(_session, Model);
        }

        public override ICommandFormatter GetCommandFormatter()
        {
            return new CommandFormatter(_session);
        }

        public override IResponseReader GetResponseReader()
        {
            return new ResponseReader(_session, Model);
        }

        public override IRequestWriter GetRequestWriter(Lazy<IBatchWriter> deferredBatchWriter)
        {
            return new RequestWriter(_session, Model, deferredBatchWriter);
        }

        public override IBatchWriter GetBatchWriter(IDictionary<object, IDictionary<string, object>> batchEntries)
        {
            return new BatchWriter(_session, batchEntries);
        }
    }
}