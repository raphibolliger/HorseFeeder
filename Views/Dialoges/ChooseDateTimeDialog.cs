using Avalonia.Controls;
using Avalonia.Data;
using HorseFeederAvalonia.Enums;
using HorseFeederAvalonia.ViewModels;
using ReactiveUI;
using System;
using System.Diagnostics;

namespace HorseFeederAvalonia.Views.Dialoges
{
    public class ChooseDateTimeDialog : Window
    {
        public ChooseDateTimeDialog()
        {
            DataContext = new ChooseDateTimeDialogViewModel();
            Width = 200;
            Height = 350;

            var chooseTopStackPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            };

            var numberInputStackPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
            };
            numberInputStackPanel.Children.Add(new NumericUpDown
            {
                [!NumericUpDown.ValueProperty] = new Binding(nameof(ChooseDateTimeDialogViewModel.SelectedHour)),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
            });
            numberInputStackPanel.Children.Add(new NumericUpDown
            {
                [!NumericUpDown.ValueProperty] = new Binding(nameof(ChooseDateTimeDialogViewModel.SelectedMinute)),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
            });

            var actionsStackPanel = new StackPanel
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
            };
            actionsStackPanel.Children.Add(new RadioButton
            {
                GroupName = "RepetitionFrequency",
                Content = "Einmalig",
                [!RadioButton.IsCheckedProperty] = new Binding(nameof(ChooseDateTimeDialogViewModel.Single))
            });
            actionsStackPanel.Children.Add(new RadioButton {
                GroupName = "RepetitionFrequency",
                Content = "Täglich",
                [!RadioButton.IsCheckedProperty] = new Binding(nameof(ChooseDateTimeDialogViewModel.Daily))
            });
            actionsStackPanel.Children.Add(new RadioButton
            {
                GroupName = "RepetitionFrequency",
                Content = "Wöchentlich",
                [!RadioButton.IsCheckedProperty] = new Binding(nameof(ChooseDateTimeDialogViewModel.Weekly))
            });

            actionsStackPanel.Children.Add(new Button
            {
                Content = "Speichern",
                Command = ReactiveCommand.Create(Save)
            });
            actionsStackPanel.Children.Add(new Button
            {
                Content = "Abbrechen",
                Command = ReactiveCommand.Create(Close)
            });

            void Save()
            {
                if (DataContext is ChooseDateTimeDialogViewModel viewModel)
                {
                    try
                    {
                        DateTime dateTime = new DateTime(viewModel.SelectedDate.Year, viewModel.SelectedDate.Month, viewModel.SelectedDate.Day, viewModel.SelectedHour, viewModel.SelectedMinute, 0);
                        RepetitionFrequency? repetitionFrequency = null;
                        
                        if (viewModel.Daily)
                        {
                            repetitionFrequency = RepetitionFrequency.Daily;
                        }
                        if (viewModel.Weekly)
                        {
                            repetitionFrequency = RepetitionFrequency.Weekly;
                        }

                        var result = new DateTimeDialogResult
                        {
                            SelectedDate = dateTime,
                            RepetitionFrequency = repetitionFrequency
                        };

                        this.Close(result);
                    }
                    catch
                    {
                        Debug.Write("Wrong date values selected");
                        this.Close();
                    }
                }
            }

            chooseTopStackPanel.Children.Add(new Calendar
            {
                [!Calendar.SelectedDateProperty] = new Binding(nameof(ChooseDateTimeDialogViewModel.SelectedDate)),
                SelectionMode = CalendarSelectionMode.SingleDate,
                DisplayDateStart = DateTime.Now,
            });
            chooseTopStackPanel.Children.Add(numberInputStackPanel);
            chooseTopStackPanel.Children.Add(actionsStackPanel);

            this.Content = chooseTopStackPanel;
        }
    }

    public class DateTimeDialogResult
    {
        public DateTime? SelectedDate { get; set; }
        public RepetitionFrequency? RepetitionFrequency { get; set; }
    }
}
