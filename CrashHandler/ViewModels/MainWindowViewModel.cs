using EnvDTE80;
using Livet;
using Livet.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CrashHandler.ViewwModels
{
    public class CallStackViewModel : ViewModel
    {
        public ViewModelCommand OpenWithVSCommand
        {
            get
            {
                if (_OpenWithVSCommand == null)
                {
                    _OpenWithVSCommand = new ViewModelCommand(OpenWithVS, () => File.Exists(SourceFile));
                }
                return _OpenWithVSCommand;
            }
        }
        ViewModelCommand _OpenWithVSCommand = null;

        public ViewModelCommand DoubleClickedCommand
        {
            get
            {
                if (_DoubleClickedCommand == null)
                {
                    _DoubleClickedCommand = new ViewModelCommand(OpenWithVS, () => File.Exists(SourceFile));
                }
                return _DoubleClickedCommand;
            }
        }
        ViewModelCommand _DoubleClickedCommand = null;

        public string Address { get; }
        public string CallStack { get; }
        public string SourceLine { get; }
        public string SourceFile { get; }

        public CallStackViewModel(string address, string callStack, string sourceLine, string sourceFile)
        {
            Address = address;
            CallStack = callStack;
            SourceLine = sourceLine;
            SourceFile = sourceFile;
        }

        void OpenWithVS()
        {
            try
            {
                EnvDTE.DTE dte = (EnvDTE.DTE)Marshal.GetActiveObject("VisualStudio.DTE");

                dte.MainWindow.Activate();

                EnvDTE.Window window = dte.ItemOperations.OpenFile(SourceFile, "{7651A703-06E5-11D1-8EBD-00A0C90F26EA}");
                ((EnvDTE.TextSelection)dte.ActiveDocument.Selection).GotoLine(int.Parse(SourceLine), true);
            }
#pragma warning disable IDE0059 // 値の不必要な代入
            catch (Exception e)
#pragma warning restore IDE0059 // 値の不必要な代入
            {
                var processInfo = new ProcessStartInfo("cmd.exe", "/c " + $"code --goto {SourceFile}:{SourceLine}");
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;

                Process.Start(processInfo);
            }
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
            if (StartupInformation.Args.Count() == 0)
            {
                _CallStackList.Add(new CallStackViewModel("0", "test", "1", @"C:\Users\Jinten\Desktop\develop\CrashHandler\CrashTest\main.cpp"));
                return;
            }

            var args = StartupInformation.Args;
            int nArgs = StartupInformation.Args.Length;

            var sb = new StringBuilder();
            for (int i = 1; i < nArgs; i += 4)
            {
                var address = "0x" + long.Parse(args[i + 0]).ToString("x4").ToString();
                var callStack = args[i + 1];
                var sourceLine = args[i + 2];
                var sourceFile = args[i + 3];

                if (i + 4 < nArgs)
                {
                    sb.AppendLine($"{callStack} : line {sourceLine} - {sourceFile}");
                }
                else
                {
                    sb.Append($"{callStack} : line {sourceLine} - {sourceFile}");
                }

                _CallStackList.Add(new CallStackViewModel(address, callStack, sourceLine, sourceFile));
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
