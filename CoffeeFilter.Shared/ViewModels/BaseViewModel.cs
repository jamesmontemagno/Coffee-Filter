using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CoffeeFilter.Shared.ViewModels
{
	public class BaseViewModel : INotifyPropertyChanged
	{
		public BaseViewModel ()
		{
		}

		private bool isBusy;
		public bool IsBusy
		{
			get { return isBusy; }
			set { SetProperty (ref isBusy, value); }
		}

		protected void SetProperty<T>(
			ref T backingStore, T value,
			[CallerMemberName]string propertyName = null,
			Action onChanged = null) 
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value)) 
				return;

			backingStore = value;

			if (onChanged != null) 
				onChanged();

			OnPropertyChanged(propertyName);
		}

		#region INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged == null)
				return;

			PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
		}
	}
}

