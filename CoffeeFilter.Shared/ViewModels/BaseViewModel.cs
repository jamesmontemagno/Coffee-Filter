using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CoffeeFilter.Shared.ViewModels
{
	public class BaseViewModel : INotifyPropertyChanged
	{
		bool isBusy;

		public bool IsBusy {
			get {
				return isBusy;
			}
			set {
				SetProperty (ref isBusy, value);
			}
		}

		protected void SetProperty<T> (
			ref T backingStore, T value,
			[CallerMemberName]string propertyName = null,
			Action onChanged = null) 
		{
			if (EqualityComparer<T>.Default.Equals (backingStore, value))
				return;

			backingStore = value;

			if (onChanged != null) 
				onChanged ();

			OnPropertyChanged(propertyName);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged (string propertyName)
		{
			if (PropertyChanged == null)
				return;

			PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
		}
	}
}

