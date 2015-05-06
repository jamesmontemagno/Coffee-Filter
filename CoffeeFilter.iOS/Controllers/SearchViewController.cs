using System;
using System.Collections.Generic;
using System.Linq;

using CloudKit;
using Foundation;
using UIKit;

namespace CoffeeFilter.iOS
{
	public class SearchViewController : UITableViewController
	{
		public Action<string> OnSearch;

		const string CellId = "SearchResultCell";

		List<CKRecord> previousSearchRequests;
		UISearchBar searchBar;
		CloudManager cloudManager;

		public SearchViewController ()
		{
			previousSearchRequests = new List<CKRecord> ();

			if (CKContainer.DefaultContainer != null)
				cloudManager = new CloudManager ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			if (cloudManager != null)
				cloudManager.FetchRecords ("SearchRequest", results => {
					previousSearchRequests = results;
					TableView.ReloadData ();
				});

			NavigationItem.HidesBackButton = true;
			TableView.Source = new TableSource (this);
			TableView.AllowsMultipleSelectionDuringEditing = false;

			searchBar = new UISearchBar {
				Placeholder = "search_hint".LocalizedString ("Search text field placeholder"),
				AutocorrectionType = UITextAutocorrectionType.No,
				AutocapitalizationType = UITextAutocapitalizationType.None,
				ShowsCancelButton = true
			};

			searchBar.SizeToFit ();
			searchBar.SearchButtonClicked += (sender, e) => {
				if (cloudManager != null && 
					!previousSearchRequests.Where (record => (NSString)record ["value"] == searchBar.Text).Any ())
					SaveSearchRequest (searchBar.Text);
				
				Search ();
			};

			searchBar.CancelButtonClicked += (sender, e) => NavigationController.PopViewController (false);

			NavigationItem.TitleView = searchBar;
			searchBar.BecomeFirstResponder ();
		}

		async void SaveSearchRequest (string searchRequestValue)
		{
			var newRecord = new CKRecord ("SearchRequest");
			newRecord ["value"] = (NSString)searchRequestValue;
			await cloudManager.SaveAsync (newRecord);
		}

		void Search ()
		{
			if (OnSearch != null)
				OnSearch (searchBar.Text);

			NavigationController.PopViewController (false);
		}

		class TableSource : UITableViewSource
		{
			SearchViewController searchController;

			public TableSource (SearchViewController controller)
			{
				searchController = controller;
			}

			public override nint RowsInSection (UITableView tableView, nint section)
			{
				return searchController.previousSearchRequests.Count;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell (CellId) ?? new UITableViewCell (
					UITableViewCellStyle.Default,
					CellId
				);

				CKRecord record = searchController.previousSearchRequests [indexPath.Row];
				cell.TextLabel.Text = (NSString)record ["value"];
				cell.TextLabel.TextColor = UIColor.FromRGB (62, 39, 35);

				return cell;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				var cell = tableView.CellAt (indexPath);
				searchController.searchBar.Text = cell.TextLabel.Text;
				searchController.Search ();
			}

			public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
			{
				return true;
			}

			public override async void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				if (editingStyle != UITableViewCellEditingStyle.Delete)
					return;

				if (searchController.cloudManager != null)
					await searchController.cloudManager.DeleteAsync (searchController.previousSearchRequests [indexPath.Row]);
				
				searchController.previousSearchRequests.RemoveAt (indexPath.Row);
				tableView.DeleteRows (new [] { indexPath }, UITableViewRowAnimation.Fade);
			}
		}
	}
}

