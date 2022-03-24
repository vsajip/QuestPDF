﻿using QuestPDF.Fluent;
using System.Diagnostics;
using ReactiveUI;
using QuestPDF.Infrastructure;
using Unit = System.Reactive.Unit;

namespace QuestPDF.Previewer
{
    internal class PreviewerWindowViewModel : ReactiveObject
    {
        public DocumentRenderer DocumentRenderer { get; } = new();

        private IDocument? _document;
        public IDocument? Document
        {
            get => _document;
            set
            {
                this.RaiseAndSetIfChanged(ref _document, value);
                UpdateDocument(value);
            }
        }

        private float _currentScroll;
        public float CurrentScroll
        {
            get => _currentScroll;
            set => this.RaiseAndSetIfChanged(ref _currentScroll, value);
        }

        private float _scrollViewportSize;
        public float ScrollViewportSize
        {
            get => _scrollViewportSize;
            set
            {
                this.RaiseAndSetIfChanged(ref _scrollViewportSize, value);
                VerticalScrollbarVisible = value < 1;
            }
        }

        private bool _verticalScrollbarVisible;
        public bool VerticalScrollbarVisible
        {
            get => _verticalScrollbarVisible;
            private set => this.RaiseAndSetIfChanged(ref _verticalScrollbarVisible, value);
        }

        public ReactiveCommand<Unit, Unit> ShowPdfCommand { get; }

        public PreviewerWindowViewModel()
        {
            HotReloadManager.UpdateApplicationRequested += InvalidateDocument;
            ShowPdfCommand = ReactiveCommand.Create(ShowPdf);
        }

        public void UnregisterHotReloadHandler()
        {
            HotReloadManager.UpdateApplicationRequested -= InvalidateDocument;
        }

        private void InvalidateDocument(object? sender, EventArgs e)
        {
            UpdateDocument(Document);
        }

        private Task UpdateDocument(IDocument? document)
        {
            return Task.Run(() => DocumentRenderer.UpdateDocument(document));
        }

        private void ShowPdf()
        {
            var path = Path.Combine(Path.GetTempPath(), ".pdf");

            try
            {
                Document?.GeneratePdf(path);
            }
            catch (Exception exception)
            {
                new ExceptionDocument(exception).GeneratePdf(path);
            }

            var openBrowserProcess = new Process
            {
                StartInfo = new()
                {
                    UseShellExecute = true,
                    FileName = path
                }
            };

            openBrowserProcess.Start();
        }
    }
}
