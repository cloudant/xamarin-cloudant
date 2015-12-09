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
    /// Marshalls a list of <see cref="IBM.Cloudant.Client.DocumentRevision"/> objects from JSON.
    /// It expects the list of documents to be located in the <c>docs</c> field of the JSON.
    /// </summary>
    internal class QueryDocumentRevisionConverter : JsonConverter
    {
        public QueryDocumentRevisionConverter ()
        {
        }

        public override bool CanConvert (Type objectType)
        {
            return objectType == typeof(IList<DocumentRevision>);
        }


        public override object ReadJson (JsonReader reader,
                                         Type objectType,
                                         object existingValue,
                                         JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            IList<DocumentRevision> documents = new List<DocumentRevision> ();

            var docs = jsonObject.GetValue("docs");
            foreach (var obj in docs) {
                var docReader = obj.CreateReader();
                var document = serializer.Deserialize<DocumentRevision>(docReader);
                documents.Add(document);
            }

            return documents;
        }

        public override bool CanWrite {
            get {
                return false;
            }
        }


        public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
        {

            throw new NotSupportedException ();
        }
    }
}

