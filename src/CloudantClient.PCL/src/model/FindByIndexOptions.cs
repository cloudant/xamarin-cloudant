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
using System.Collections.Generic;

namespace Com.Cloudant.Client.Model
{
	public class FindByIndexOptions
	{
		// search fields
		private Int32 plimit;
		private Int32 pskip;
		private List<IndexField> psort = new List<IndexField>();
		private List<String> pfields = new List<String>();
		private Int32 preadQuorum;
		private String puseIndex = null;


		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Cloudant.Client.Model.FindByIndexOptions"/> class.
		/// </summary>
		public FindByIndexOptions ()
		{
		}


		/// <summary>
		/// limit the number of results return
		/// </summary>
		/// <param name="limit">Max number of results</param>
		public FindByIndexOptions limit(Int32 limit) {
			this.plimit = limit;
			return this;
		}


		/// <summary>
		/// Skips <i>n</i> number of results.
		/// </summary>
		/// <param name="skip">Number of results to skip</param>
		public FindByIndexOptions skip(Int32 skip) {
			this.pskip = skip;
			return this;
		}


		/// <summary>
		/// Sets the readQuorum
		/// </summary>
		/// <param name="readQuorum">The read quorum</param>
		public FindByIndexOptions readQuorum(Int32 readQuorum) {
			this.preadQuorum = readQuorum;
			return this;
		}


		/// <summary>
		/// Can be called multiple times to set the list of return fields
		/// </summary>
		/// <param name="field">set the return fields</param>
		public FindByIndexOptions fields(String field) {
			this.pfields.Add(field);
			return this;
		}

			
		/// <summary>
		/// Can be called multiple times to set the sort syntax
		/// </summary>
		/// <param name="sort">add a sort syntax field</param>
		public FindByIndexOptions sort(IndexField sort) {
			this.psort.Add(sort);
			return this;
		}


		/// <summary>
		/// Specify a specific index to run the query against
		/// </summary>
		/// <param name="designDocument">set the design document to use</param>
		private FindByIndexOptions useIndex(String designDocument) {
			this.puseIndex = "\"" + designDocument + "\"";
			return this;
		}


		/// <summary>
		/// Specify a specific index to run the query against
		/// </summary>
		/// <returns>The index.</returns>
		/// <param name="designDocument">set the design document to use</param>
		/// <param name="indexName">set the index name to use</param>
		public FindByIndexOptions useIndex(String designDocument, String indexName) {
			this.puseIndex = "[\"" + designDocument + "\",\"" + indexName + "\"]";
			return this;
		}

		public List<String> getFields() {
			return pfields;
		}

		public List<IndexField> getSort() {
			return psort;
		}

		public Int32 getLimit() {
			return plimit;
		}

		public Int32 getSkip() {
			return pskip;
		}

		public Int32 getReadQuorum() {
			return preadQuorum;
		}

		public String getUseIndex() {
			return puseIndex;
		}


	}
}

