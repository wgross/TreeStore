using GalaSoft.MvvmLight.Command;
using Kosmograph.Desktop.EditModel;
using Kosmograph.Model;
using System;

namespace Kosmograph.Desktop.ViewModel
{
    public class ShowTagsViewModel
    {
        public ShowTagsViewModel()
        {
            this.EditCommand = new RelayCommand<Tag>(this.EditExecuted);
        }

        public RelayCommand<Tag> EditCommand { get; }

        private void EditExecuted(Tag obj)
        {
            //
        }

        public TagEditModel Edited { get; set; }
    }
}