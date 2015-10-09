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

namespace Com.Cloudant.Client
{
	/// <summary>
	/// Exception type used by the Cloudant Client to report exceptions
	/// while working with data services.
	/// </summary>
	public class DataException: Exception
	{
		private static readonly string CLOUDANT_DOMAIN = "Cloudant";

		public static readonly int Database_ObjectMappingError = 001;
		public static readonly int Database_QueryError = 002;

		public static readonly int Account_InvalidAccountSettings = 010;

		public static readonly int Database_DatabaseModificationFailure = 100;
		public static readonly int Database_SaveDocumentRevisionFailure =       102;
		public static readonly int Database_FetchDocumentRevisionFailure =      103;
		public static readonly int Database_DeleteDocumentRevisionFailure =     104;
		public static readonly int Database_IndexModificationFailure = 105;

		public static readonly int CLOUDANT_HTTP_ERROR = 400;
		public static readonly int CLOUDANT_UNAUTHORIZED_FAILURE = 500 ;

		public static readonly int DataObjectMapper_ObjectNotProvided = 600;
		public static readonly int DataObjectMapper_NoRegisteredDataType = 601;
		public static readonly int DataObjectMapper_ObjectDoesntImplement = 602;
		public static readonly int DataObjectMapper_DataTypeNotProvided = 603;
		public static readonly int DataObjectMapper_RevisionNotProvided = 604;
		public static readonly int DataObjectMapper_MissingDataType = 606;
		public static readonly int DataObjectMapper_MissingMetadata = 607;

		public static readonly int CloudantQueryOperation_PerformQueryError = 700;

		protected string domain { get; }
		protected int code { get; }


		public DataException(int code, String message) : base(message){
			this.code = code;
		}

		public DataException(int code, String message, Exception cause) : base(message, cause) {
			this.code = code;
		}

	}
}

