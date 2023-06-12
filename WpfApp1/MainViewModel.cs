using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace WpfApp1;

class MainViewModel : INotifyPropertyChanged
{
    private Student _selectedPerson;
    public ObservableCollection<Student> Students { get; set; }

    public MainViewModel()
    {
        Students = new ObservableCollection<Student>();

    }
    public Student SelectedPerson
    {
        get { return _selectedPerson; }
        set
        {
            _selectedPerson = value;
            OnPropertyChanged("SelectedPerson");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(prop));

    }

    private RelayCommand saveFileCommand;
    public RelayCommand SaveFileCommand
    {
        get
        {
            return saveFileCommand ??
              (saveFileCommand = new RelayCommand(obj =>
              {
                  if (obj == null)
                  {
                      SaveWindow saveWindow = new SaveWindow();
                      saveWindow.ShowDialog();
                      return;
                  }

                  SaveFileDialog saveFileDialog = new SaveFileDialog();
                  saveFileDialog.FileName = SelectedPerson.Name + SelectedPerson.SecondName;
                  saveFileDialog.DefaultExt = ".txt";

                  if (saveFileDialog.ShowDialog() == true)
                  {
                      Student user = obj as Student;
                      var options = new JsonSerializerOptions
                      {
                          Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                          WriteIndented = true,
                          DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                      };

                      var json = JsonSerializer.Serialize(user, options);
                      File.WriteAllText(saveFileDialog.FileName, json);
                  }
              }));
        }
    }

    public async void AddStudent(Student student, Stream photo)
    {

        if (!Students.Contains(student))
        {
            var filePath = $"C:\\Users\\Admin\\OneDrive\\Рабочий стол\\Новая Папка\\{student.SecondName + student.Name}.jpg";
            using var file = File.OpenWrite(filePath);
            await photo.CopyToAsync(file);
            student.ImageCamera = $"C:\\Users\\Admin\\OneDrive\\Рабочий стол\\Новая Папка\\{student.SecondName + student.Name}.jpg";
            student.Image = $"C:\\Users\\Admin\\OneDrive\\Рабочий стол\\Новая Папка\\{student.Name + student.SecondName}.jpg";
            Students.Add(student);
        }
    }
}