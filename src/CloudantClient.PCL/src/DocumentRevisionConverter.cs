//
//  Copyright (c) 2015 IBM Corp. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
//  except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software distributed under the
//  License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
//  either express or implied. See the License for the specific language governing permissions 
//  and limitations under the License.
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace IBM.Cloudant.Client
{
    /// <summary>
    /// Marshalls a <see cref="IBM.Cloudant.Client.DocumentRevision"/> object to and from JSON.
    /// </summary>
    internal class DocumentRevisionConverter : JsonConverter
    {
        public DocumentRevisionConverter()
        {
        }


        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DocumentRevision);
        }


        public override object ReadJson(JsonReader reader,
                                        Type objectType,
                                        object existingValue,
                                        JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var document = new DocumentRevision();
            document.body = new Dictionary<string,object>();

            foreach (var kv in jsonObject)
            {
                switch (kv.Key)
                {
                    case "_id":
                    case "id":
                        document.docId = kv.Value.ToObject<string>();
                        break;
                    case "_rev":
                    case "rev":
                        document.revId = kv.Value.ToObject<string>();
                        break;
                    case "_deleted":
                        document.isDeleted = kv.Value.ToObject<bool>();
                        break;
                    default:
                    // Other values go in the body
                        document.body.Add(kv.Key, kv.Value.ToObject<dynamic>());
                        break;
                }
            }
                

            return document;
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var document = value as DocumentRevision;
            writer.WriteStartObject();

            if (document.docId != null)
            {
                writer.WritePropertyName("_id");
                serializer.Serialize(writer, document.docId);
            }

            if (document.revId != null)
            {
                writer.WritePropertyName("_rev");
                serializer.Serialize(writer, document.revId);
            }
            if (document.isDeleted)
            {
                writer.WritePropertyName("_deleted");
                serializer.Serialize(writer, document.isDeleted);
            }

            foreach (var pair in document.body)
            {
                writer.WritePropertyName(pair.Key);
                serializer.Serialize(writer, pair.Value);
            }

            writer.WriteEnd();
        }


    }
}

