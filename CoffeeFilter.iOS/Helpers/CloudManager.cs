using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CloudKit;
using Foundation;

namespace CoffeeFilter
{
	public class CloudManager : NSObject
	{
		const string ValueField = "value";

		CKContainer container;
		CKDatabase privateDatabase;

		public CloudManager ()
		{
			container = CKContainer.DefaultContainer;
			privateDatabase = container.PrivateCloudDatabase;
		}

		public void FetchRecords (string recordType, Action<List<CKRecord>> completionHandler)
		{
			var truePredicate = NSPredicate.FromValue (true);
			var query = new CKQuery (recordType, truePredicate) {
				SortDescriptors = new [] { new NSSortDescriptor ("creationDate", false) }
			};

			var queryOperation = new CKQueryOperation (query) {
				DesiredKeys = new [] { ValueField }
			};

			var results = new List<CKRecord> ();

			queryOperation.RecordFetched = (record) => { 
				results.Add (record);
			};

			queryOperation.Completed = (cursor, error) => {
				if (error != null) {
					Console.WriteLine ("An error occured: {0}", error.Description);
					return;
				}

				InvokeOnMainThread (() => completionHandler (results));
			};

			privateDatabase.AddOperation (queryOperation);
		}

		public async Task SaveAsync (CKRecord record)
		{
			try {
				await privateDatabase.SaveRecordAsync (record);
			} catch (Exception e) {
				Console.WriteLine ("An error occured: {0}", e.Message);
			}
		}

		public async Task DeleteAsync (CKRecord record)
		{
			try {
				await privateDatabase.DeleteRecordAsync (record.Id);
			} catch (Exception e) {
				Console.WriteLine ("An error occured: {0}", e.Message);
			}
		}
	}
}

