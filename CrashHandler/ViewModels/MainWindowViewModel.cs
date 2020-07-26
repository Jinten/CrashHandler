using Livet;
using Livet.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CrashHandler.ViewwModels
{
    public class CallStackViewModel : ViewModel
    {
        public string CallStack { get; }
        public string LineNumber { get; }
        public string FileName { get; }

        public CallStackViewModel(string callStack, string lineNumber, string fileName)
        {
            CallStack = callStack;
            LineNumber = lineNumber;
            FileName = fileName;
        }
    }

    public class MainWindowViewModel : ViewModel
    {
        public ViewModelCommand CopyCommand
        {
            get
            {
                if (_CopyCommand == null)
                {
                    _CopyCommand = new ViewModelCommand(() => Clipboard.SetText(RawCallStack));
                }
                return _CopyCommand;
            }
        }
        ViewModelCommand _CopyCommand = null;

        public ViewModelCommand DebugCommand
        {
            get
            {
                if (_DebugCommand == null)
                {
                    _DebugCommand = new ViewModelCommand(Debug, () => File.Exists(DumpedPath));
                }
                return _DebugCommand;
            }
        }
        ViewModelCommand _DebugCommand = null;

        public ViewModelCommand OpenWithExplorerCommand
        {
            get
            {
                if (_OpenWithExplorerCommand == null)
                {
                    _OpenWithExplorerCommand = new ViewModelCommand(OpenWithExplorer, () => File.Exists(DumpedPath));
                }
                return _OpenWithExplorerCommand;
            }
        }
        ViewModelCommand _OpenWithExplorerCommand = null;

        public ViewModelCommand DeleteDumpFileCommand
        {
            get
            {
                if (_DeleteDumpFileCommand == null)
                {
                    _DeleteDumpFileCommand = new ViewModelCommand(DeleteDumpFile, () => File.Exists(DumpedPath));
                }
                return _DeleteDumpFileCommand;
            }
        }
        ViewModelCommand _DeleteDumpFileCommand = null;

        public ViewModelCommand ExitCommand
        {
            get
            {
                if (_ExitCommand == null)
                {
                    _ExitCommand = new ViewModelCommand(Exit);
                }
                return _ExitCommand;
            }
        }
        ViewModelCommand _ExitCommand = null;

        public IEnumerable<CallStackViewModel> CallStackList => _CallStackList;
        ObservableCollection<CallStackViewModel> _CallStackList = new ObservableCollection<CallStackViewModel>();

        public string RawCallStack
        {
            get => _RawCallStack;
            set => RaisePropertyChangedIfSet(ref _RawCallStack, value);
        }
        string _RawCallStack;

        public string DumpedPath
        {
            get => _DumpedPath;
            set => RaisePropertyChangedIfSet(ref _DumpedPath, value);
        }
        string _DumpedPath;

        public MainWindowViewModel()
        {
            var args = StartupInformation.Args;
            int nArgs = StartupInformation.Args.Length;

            var sb = new StringBuilder();
            for (int i = 1; i < nArgs; i += 3)
            {
                var callStack = $"{args[i + 0]} ()";
                var lineNumber = args[i + 1];
                var fileName = args[i + 2];

                if (i + 3 < nArgs)
                {
                    sb.AppendLine($"{callStack} : line {lineNumber} - {fileName}");
                }
                else
                {
                    sb.Append($"{callStack} : line {lineNumber} - {fileName}");
                }

                _CallStackList.Add(new CallStackViewModel(callStack, lineNumber, fileName));
            }

            RawCallStack = sb.ToString();

            // 0番目には、出力されたダンプファイルが存在する
            DumpedPath = StartupInformation.Args[0];
        }

        void Debug()
        {
            Process.Start(DumpedPath);
        }

        void OpenWithExplorer()
        {
            Process.Start("explorer.exe", $@"/select,{DumpedPath}");
        }

        void DeleteDumpFile()
        {
            var result = MessageBox.Show("Are you sure?", "Confirm", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                File.Delete(DumpedPath);

                DebugCommand.RaiseCanExecuteChanged();
                OpenWithExplorerCommand.RaiseCanExecuteChanged();
                DeleteDumpFileCommand.RaiseCanExecuteChanged();
            }
        }

        void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}
