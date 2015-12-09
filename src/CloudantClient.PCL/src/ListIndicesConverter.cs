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
    /// Marshals a list of <see cref="IBM.Cloudant.client.Index"/> objects from json.
    /// </summary>
    internal class ListIndicesConverter : JsonConverter
    {
        public ListIndicesConverter ()
        {
        }

        public override bool CanConvert (Type objectType)
        {
            return objectType == typeof(IList<Index>);
        }


        public override object ReadJson (JsonReader reader,
                                         Type objectType,
                                         object existingValue,
                                         JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            IList<Index> indexes = new List<Index> ();

            var indexJsonObject = jsonObject.GetValue("indexes");
            foreach (var obj in indexJsonObject) {
                var indexReader = obj.CreateReader();
                var index = serializer.Deserialize<Index>(indexReader);
                indexes.Add(index);
            }

            return indexes;
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

