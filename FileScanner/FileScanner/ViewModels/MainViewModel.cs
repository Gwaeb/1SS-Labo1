using FileScanner.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace FileScanner.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string selectedFolder;
        
         
        public DelegateCommand<string> OpenFolderCommand { get; private set; }
        public DelegateCommand<string> ScanFolderCommand { get; private set; }


        private ObservableCollection<Folder> folderItems = new ObservableCollection<Folder>();
        public ObservableCollection<Folder> FolderItems { 
            get => folderItems;
            set 
            { 
                folderItems = value;
                OnPropertyChanged();
            }
        }



        public string SelectedFolder
        {
            get => selectedFolder;
            set
            {
                selectedFolder = value;
                OnPropertyChanged();
                ScanFolderCommand.RaiseCanExecuteChanged();
            }
        }

        public MainViewModel()
        {
            OpenFolderCommand = new DelegateCommand<string>(OpenFolder);
            ScanFolderCommand = new DelegateCommand<string>(ScanFolder, CanExecuteScanFolder);
        }

        private bool CanExecuteScanFolder(string obj)
        {
            return !string.IsNullOrEmpty(SelectedFolder);
        }

        private void OpenFolder(string obj)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    SelectedFolder = fbd.SelectedPath;
                }
            }
        }

        private void ScanFolder(string dir)
        {
            Task.Run(() =>
            {
                try
                {
                    FolderItems = new ObservableCollection<Folder>(GetDirs(dir));
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (PathTooLongException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            
        }

        IEnumerable<Folder> GetDirs(string dir)
        {
            foreach (var d in Directory.EnumerateDirectories(dir, "*"))
            {
                IEnumerable<string> files;
                try
                {
                   files  = Directory.EnumerateFiles(d, "*");
                }
                catch (Exception)
                {
                    continue;
                }
                

                foreach (var file in files)
                {
                    //yield return file;
                    yield return new Folder("/Views/file.png", file);
                }
                var temp = GetDirs(d);
                foreach (var dd in temp)
                {
                    yield return dd;

                }
                //yield return d;
                yield return new Folder("/Views/folder.png", d);
            }
        }

        ///TODO : Tester avec un dossier avec beaucoup de fichier
        ///TODO : Rendre l'application asynchrone
        ///TODO : Ajouter un try/catch pour les dossiers sans permission


    }

    public class Folder
    {
        public string image { get; set; }
        public string path { get; set; }

        public Folder(string image, string path)
        {
            this.image = image;
            this.path = path;
        }
    }
}
