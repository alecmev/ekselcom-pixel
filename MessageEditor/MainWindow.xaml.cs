using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;
using System.Windows.Controls.Primitives;
using System.Xml;
using MyFa;
using MyFa.Properties;
using System.Timers;

namespace MyFa
{
    public partial class MessageEditor : Window
    {
        private MessageCollection Lines;
        private MessageCollection Destinations;
        private MessageCollection Stops;
        private PagePreview SelectedMessagePreview;

        private SolidColorBrush ItemExists;
        private Timer DestinationTimer;
        private Timer StopTimer;
        private delegate void TimerTick();

        private bool AllInitialized = true;

        private string SaveFile = "";
        private bool isSaved = true;
        public bool IsSaved
        {
            get { return isSaved; }
            set
            {
                isSaved = value;
                Title = "MyFaPixel - Message Editor - " + ((SaveFile != "") ? (SaveFile.Substring(SaveFile.LastIndexOf(@"\") + 1) + (value ? "" : " *")) : "untitled");
            }
        }

        public MessageEditor()
        {
            ItemExists = new SolidColorBrush(Color.FromArgb(255, 255, 204, 0));

            DestinationTimer = new Timer();
            DestinationTimer.AutoReset = false;
            DestinationTimer.Elapsed += new ElapsedEventHandler(DestinationTimerElapsed);

            StopTimer = new Timer();
            StopTimer.AutoReset = false;
            StopTimer.Elapsed += new ElapsedEventHandler(StopTimerElapsed);

            InitializeComponent();

            Closing += new System.ComponentModel.CancelEventHandler(MessageEditorClosing);
        }

        void MessageEditorClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsSaved)
            {
                MessageBoxResult result = MessageBox.Show("Current message collection is not saved. Do you want to save?", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel) e.Cancel = true;
                if (result == MessageBoxResult.Yes)
                {
                    SaveClick(null, null);
                    if (!IsSaved) e.Cancel = true;
                }
            }
        }

        #region Menu
        private void InitializeAll()
        {
            AllInitialized = false;

            SelectedMessagePreview = new PagePreview(Settings.Default.DisplayWidth, Settings.Default.DisplayHeight, new Int32Rect(0, 0, Settings.Default.DisplayWidth, Settings.Default.DisplayHeight));
            SelectedMessagePreview.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            PagePreviewScroll.Content = SelectedMessagePreview;

            ComboBoxItem tmpItem;

            LineSelector.Items.Clear();
            for (int i = 0; i < Settings.Default.MessageLinesCount; ++i)
            {
                tmpItem = new ComboBoxItem();
                tmpItem.Content = i + " line";
                if (ExistsLine(i)) tmpItem.Background = ItemExists;
                LineSelector.Items.Add(tmpItem);
                UpdateLineText(i);
            }

            DestinationSelector.Items.Clear();
            for (int i = 0; i < Settings.Default.MessageDestinationsCount; ++i)
            {
                tmpItem = new ComboBoxItem();
                tmpItem.Content = i + " destination";
                if (ExistsDestination(i)) tmpItem.Background = ItemExists;
                DestinationSelector.Items.Add(tmpItem);
                UpdateDestinationText(i);
            }

            StopSelector.Items.Clear();
            for (int i = 0; i < Settings.Default.MessageStopsCount; ++i)
            {
                tmpItem = new ComboBoxItem();
                tmpItem.Content = i + " stop";
                if (ExistsStop(i)) tmpItem.Background = ItemExists;
                StopSelector.Items.Add(tmpItem);
                UpdateStopText(i);
            }

            DestinationPageSelector.Items.Clear();
            StopPageSelector.Items.Clear();
            for (int i = 0; i < Settings.Default.MessagePageCount; ++i)
            {
                tmpItem = new ComboBoxItem();
                tmpItem.Content = (i + 1) + " page";
                DestinationPageSelector.Items.Add(tmpItem);
                UpdateDestinationPageText(0, i);

                tmpItem = new ComboBoxItem();
                tmpItem.Content = (i + 1) + " page";
                StopPageSelector.Items.Add(tmpItem);
                UpdateStopPageText(0, i);
            }

            LineSelector.SelectedIndex = 0;
            DestinationSelector.SelectedIndex = 0;
            StopSelector.SelectedIndex = 0;
            DestinationPageSelector.SelectedIndex = 0;
            StopPageSelector.SelectedIndex = 0;

            PagePreviewScroll.Visibility = System.Windows.Visibility.Visible;
            PreviewSelectScroll.Visibility = System.Windows.Visibility.Visible;

            AllInitialized = true;
        }

        private void NewClick(object sender, RoutedEventArgs e)
        {
            if (IsSaved ? true : (MessageBox.Show("Current message collection is not saved. Continue?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes))
            {
                Lines = new MessageCollection("Lines", Settings.Default.MessageLinesCount);
                Destinations = new MessageCollection("Destinations", Settings.Default.MessageDestinationsCount);
                Stops = new MessageCollection("Stops", Settings.Default.MessageStopsCount);

                InitializeAll();

                SaveFile = "";
                IsSaved = false;
            }
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            if (IsSaved ? true : (MessageBox.Show("Current message collection is not saved. Continue?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes))
            {
                MessageOpenWindow mow = new MessageOpenWindow();
                mow.Owner = this;

                if (mow.ShowDialog() == true)
                {
                    if (mow.Type == 0)
                    {
                        Stream stream = File.Open(mow.Path, FileMode.Open);
                        BinaryFormatter bformatter = new BinaryFormatter();
                        Collection<MessageCollection> tmp = (Collection<MessageCollection>)bformatter.Deserialize(stream);
                        stream.Close();

                        Lines = tmp[0];
                        Destinations = tmp[1];
                        Stops = tmp[2];

                        InitializeAll();

                        RefreshPreview();
                    }

                    SaveFile = mow.Path;
                    IsSaved = true;
                }
            }
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            if (AllInitialized)
            {
                if (!IsSaved)
                {
                    if (SaveFile != "")
                    {
                        Collection<MessageCollection> tmp = new Collection<MessageCollection>();
                        tmp.Add(Lines);
                        tmp.Add(Destinations);
                        tmp.Add(Stops);
                        Stream stream = File.Open(SaveFile, FileMode.Create);
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(stream, tmp);
                        stream.Close();

                        IsSaved = true;
                    }
                    else SaveAsClick(sender, e);
                }
            }
        }

        private void SaveAsClick(object sender, RoutedEventArgs e)
        {
            if (AllInitialized)
            {
                MessageSaveWindow msw = new MessageSaveWindow((SaveFile != "") ? SaveFile.Substring(SaveFile.LastIndexOf(@"\") + 1, SaveFile.Length - SaveFile.LastIndexOf(@"\") - 6) : "");
                msw.Owner = this;

                if (msw.ShowDialog() == true)
                {
                    if (msw.Type == 0)
                    {
                        Collection<MessageCollection> tmp = new Collection<MessageCollection>();
                        tmp.Add(Lines);
                        tmp.Add(Destinations);
                        tmp.Add(Stops);
                        Stream stream = File.Open(msw.Path, FileMode.Create);
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(stream, tmp);
                        stream.Close();

                        SaveFile = msw.Path;
                        IsSaved = true;
                    }
                    else if (msw.Type == 1)
                    {
                        Description tmp = new Description(Lines, Destinations, Stops);

                        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFaPixel\Message Collections\tmp");
                        String tmpPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFaPixel\Message Collections\tmp\tmp.bin";

                        if (File.Exists(tmpPath)) File.Delete(tmpPath);
                        Stream plain = File.Open(tmpPath, FileMode.Create);
                        tmp.Write(plain);
                        plain.Close();

                        plain = File.Open(tmpPath, FileMode.Open);
                        if (File.Exists(msw.Path)) File.Delete(msw.Path);
                        Stream prepared = File.Open(msw.Path, FileMode.Create);
                        Byte GeneralCRC = BinaryEncapsulator.Encapsulate(plain, prepared);
                        prepared.Close();
                        plain.Close();

                        plain = File.Open(tmpPath, FileMode.Append);
                        BinaryWriter binary = new BinaryWriter(plain);
                        binary.Write(GeneralCRC);
                        plain.Close();
                    }
                }
            }
        } 
        #endregion

        #region Preview
        private void RefreshPreview()
        {
            if (SelectedMessagePreview != null && AllInitialized)
            {
                SelectedMessagePreview.Clear(false);
                if (RadioA.IsChecked == true)
                {
                    if (Lines.Messages[LineSelector.SelectedIndex] != null) SelectedMessagePreview.Render(Lines.Messages[LineSelector.SelectedIndex].Pages[0]);
                    if (DestinationSelector.SelectedIndex > -1 && Destinations.Messages[DestinationSelector.SelectedIndex] != null) SelectedMessagePreview.Render(Destinations.Messages[DestinationSelector.SelectedIndex].Pages[DestinationPageSelector.SelectedIndex]);
                    else if (Lines.Messages[LineSelector.SelectedIndex] == null) SelectedMessagePreview.Clear(true);
                }
                else if (StopSelector.SelectedIndex > -1 && Stops.Messages[StopSelector.SelectedIndex] != null) SelectedMessagePreview.Render(Stops.Messages[StopSelector.SelectedIndex].Pages[StopPageSelector.SelectedIndex]);
                else SelectedMessagePreview.Clear(true);
                IsSaved = false;
            }
        }

        private void RadioChecked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                DestinationStop();
                StopStop();
                RefreshPreview();
            }
        } 
        #endregion

        #region Lines
        private bool ExistsLine(int message)
        {
            if (message != -1 && Lines.Messages[message] != null) return true;
            return false;
        }

        private void UpdateLineText(int message)
        {
            if (ExistsLine(message)) ((ComboBoxItem)LineSelector.Items[message]).Content = message + " line - " + Lines.Messages[message].Pages[0].Strings[0].Text;
            else ((ComboBoxItem)LineSelector.Items[message]).Content = message + " line (no content)";
        }

        private void EditLine(object sender, RoutedEventArgs e)
        {
            PageEditor pe = new PageEditor(Lines.Messages[LineSelector.SelectedIndex] != null ? Lines.Messages[LineSelector.SelectedIndex].Pages[0] : null, false, new Int32Rect(0, 0, Settings.Default.MessageLineWidth, Settings.Default.DisplayHeight), LineSelector.SelectedIndex, "line", 0, 1);

            if (pe.ShowDialog() == true)
            {
                MessageStorage tmpMessage = new MessageStorage(false, 1);
                tmpMessage.Pages[0] = pe.Page;
                Lines.Messages[LineSelector.SelectedIndex] = tmpMessage;
                ((ComboBoxItem)LineSelector.SelectedItem).Background = ItemExists;
                LineSelector.Background = ItemExists;
                RefreshPreview();
                UpdateLineText(LineSelector.SelectedIndex);
            }
        }

        private void ClearLine(object sender, RoutedEventArgs e)
        {
            if (Lines.Messages[LineSelector.SelectedIndex] != null && MessageBox.Show("Entry will be deleted permamently. Continue?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Lines.Messages[LineSelector.SelectedIndex] = null;
                ((ComboBoxItem)LineSelector.SelectedItem).Background = Brushes.White;
                LineSelector.Background = Brushes.White;
                RefreshPreview();
                UpdateLineText(LineSelector.SelectedIndex);
            }
        }

        private void LineChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LineSelector.SelectedItem != null && ((ComboBoxItem)LineSelector.SelectedItem).Background == ItemExists) LineSelector.Background = ItemExists;
            else LineSelector.Background = Brushes.White;
            RefreshPreview();
        } 
        #endregion

        #region Destinations
        #region Manipulation
        private bool ExistsDestination(int message)
        {
            if (message != -1 && Destinations.Messages[message] != null) return true;
            return false;
        }

        private bool ExistsDestinationPage(int message, int page)
        {
            if (ExistsDestination(message) && page != -1 && Destinations.Messages[message].Pages[page] != null) return true;
            return false;
        }

        private MessageStorage GetDestination(int message)
        {
            return Destinations.Messages[message];
        }

        private void SetDestination(int message, MessageStorage storage)
        {
            CreateDestination(message);
            Destinations.Messages[message] = storage;
        }

        private PageStorage GetDestinationPage(int message, int page)
        {
            if (ExistsDestination(message)) return Destinations.Messages[message].Pages[page];
            return null;
        }

        private void SetDestinationPage(int message, int page, PageStorage storage)
        {
            CreateDestinationPage(message, page);
            Destinations.Messages[message].Pages[page] = storage;
            UpdateDestinationPageList();
        }

        private void UpdateDestination()
        {
            if (ExistsDestination(DestinationSelector.SelectedIndex))
            {
                DestinationSelector.Background = ItemExists;
                DestinationPageLoop.IsChecked = GetDestination(DestinationSelector.SelectedIndex).Loop;
                int tmp = 0;
                for (int i = 0; i < DestinationPageSelector.Items.Count; ++i) if (ExistsDestinationPage(DestinationSelector.SelectedIndex, i)) ++tmp;
                DestinationPageCount.Text = tmp.ToString();
            }
            else
            {
                DestinationSelector.Background = Brushes.White;
                DestinationPageLoop.IsChecked = false;
                DestinationPageCount.Text = "0";
            }
            UpdateDestinationText(DestinationSelector.SelectedIndex);
        }

        private void UpdateDestinationText(int message)
        {
            if (ExistsDestination(message))
            {
                int page;
                for (page = 0; page < DestinationPageSelector.Items.Count && !ExistsDestinationPage(message, page); ++page) { }
                ((ComboBoxItem)DestinationSelector.Items[message]).Content = message + " destination - " + Destinations.Messages[message].Pages[page].Strings[0].Text;
            }
            else if (message >= 0) ((ComboBoxItem)DestinationSelector.Items[message]).Content = message + " destination (no content)";
        }

        private void UpdateDestinationPage()
        {
            if (ExistsDestinationPage(DestinationSelector.SelectedIndex, DestinationPageSelector.SelectedIndex)) DestinationPageSelector.Background = ItemExists;
            else DestinationPageSelector.Background = Brushes.White;
            UpdateDestination();
            UpdateDestinationPageText(DestinationSelector.SelectedIndex, DestinationPageSelector.SelectedIndex);
        }

        private void UpdateDestinationPageText(int message, int page)
        {
            if (ExistsDestinationPage(message, page)) ((ComboBoxItem)DestinationPageSelector.Items[page]).Content = (page + 1) + " page - " + Destinations.Messages[message].Pages[page].Strings[0].Text;
            else if (page >= 0) ((ComboBoxItem)DestinationPageSelector.Items[page]).Content = (page + 1) + " page (no content)";
        }

        private void UpdateDestinationPageList()
        {
            bool tmp = true;
            for (int i = 0; i < DestinationPageSelector.Items.Count; ++i)
            {
                if (ExistsDestinationPage(DestinationSelector.SelectedIndex, i))
                {
                    ((ComboBoxItem)DestinationPageSelector.Items[i]).Background = ItemExists;
                    tmp = false;
                }
                else ((ComboBoxItem)DestinationPageSelector.Items[i]).Background = Brushes.White;
                UpdateDestinationPageText(DestinationSelector.SelectedIndex, i);
            }
            if (tmp) RemoveDestination(DestinationSelector.SelectedIndex);
            UpdateDestinationPage();
        }

        private bool CreateDestination(int message)
        {
            if (ExistsDestination(message)) return false;
            Destinations.Messages[message] = new MessageStorage(false, Settings.Default.MessagePageCount);
            ((ComboBoxItem)DestinationSelector.Items[message]).Background = ItemExists;
            //UpdateDestination();
            return true;
        }

        private bool CreateDestinationPage(int message, int page)
        {
            if (ExistsDestinationPage(message, page)) return false;
            CreateDestination(message);
            Destinations.Messages[message].Pages[page] = new PageStorage(1, 1);
            //UpdateDestinationPageList();
            return true;
        }

        private bool RemoveDestination(int message)
        {
            if (!ExistsDestination(message)) return false;
            Destinations.Messages[message] = null;
            ((ComboBoxItem)DestinationSelector.Items[message]).Background = Brushes.White;
            UpdateDestination();
            return true;
        }

        private bool RemoveDestinationPage(int message, int page)
        {
            if (!ExistsDestinationPage(message, page)) return false;
            Destinations.Messages[message].Pages[page] = null;
            UpdateDestinationPageList();
            return true;
        } 
        #endregion

        #region Events
        private void EditDestination(object sender, RoutedEventArgs e)
        {
            DestinationStop();
            PageEditor pe = new PageEditor(ExistsDestinationPage(DestinationSelector.SelectedIndex, DestinationPageSelector.SelectedIndex) ? GetDestinationPage(DestinationSelector.SelectedIndex, DestinationPageSelector.SelectedIndex) : null, true, new Int32Rect(Settings.Default.MessageLineWidth, 0, Settings.Default.DisplayWidth - Settings.Default.MessageLineWidth, Settings.Default.DisplayHeight), DestinationSelector.SelectedIndex, "destination", DestinationPageSelector.SelectedIndex, DestinationPageSelector.Items.Count);

            if (pe.ShowDialog() == true)
            {
                SetDestinationPage(DestinationSelector.SelectedIndex, DestinationPageSelector.SelectedIndex, pe.Page);
                RefreshPreview();
            }
        }

        private void ClearDestination(object sender, RoutedEventArgs e)
        {
            DestinationStop();
            if (ExistsDestinationPage(DestinationSelector.SelectedIndex, DestinationPageSelector.SelectedIndex) && MessageBox.Show("Entry will be deleted permamently. Continue?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                RemoveDestinationPage(DestinationSelector.SelectedIndex, DestinationPageSelector.SelectedIndex);
                RefreshPreview();
            }
        }

        private void DestinationChanged(object sender, SelectionChangedEventArgs e)
        {
            DestinationStop();
            UpdateDestinationPageList();
            DestinationPageSelector.SelectedIndex = 0;
            RefreshPreview();
        }

        private void DestinationPageChanged(object sender, SelectionChangedEventArgs e)
        {
            DestinationStop();
            UpdateDestinationPage();
            RefreshPreview();
        }

        private void DestinationPageLoopChecked(object sender, RoutedEventArgs e)
        {
            if (ExistsDestination(DestinationSelector.SelectedIndex)) Destinations.Messages[DestinationSelector.SelectedIndex].Loop = (DestinationPageLoop.IsChecked == true);
            else DestinationPageLoop.IsChecked = false;
            RefreshPreview();
        }

        private void DestinationPlayClicked(object sender, RoutedEventArgs e)
        {
            if (DestinationTimer.Enabled) DestinationStop();
            else DestinationPlay();
        }

        private void DestinationPlay()
        {
            DestinationTimer.Enabled = false;
            if (ExistsDestination(DestinationSelector.SelectedIndex))
            {
                if (ExistsDestinationPage(DestinationSelector.SelectedIndex, DestinationPageSelector.SelectedIndex)) DestinationTimer.Interval = GetDestinationPage(DestinationSelector.SelectedIndex, DestinationPageSelector.SelectedIndex).Time * 1000;
                else DestinationTimer.Interval = 1;

                DestinationPlayButton.Content = "";
                DestinationTimer.Enabled = true;
            }
        }

        private void DestinationStop()
        {
            DestinationPlayButton.Content = "";
            DestinationTimer.Enabled = false;
        }

        void DestinationTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TimerTick(DestinationTimerTick));
        }

        private void DestinationTimerTick()
        {
            int index = DestinationPageSelector.SelectedIndex + 1;
            for (int i = 0; i < DestinationPageSelector.Items.Count; ++i, ++index)
            {
                if (index == DestinationPageSelector.Items.Count)
                {
                    index = 0;
                    if (!GetDestination(DestinationSelector.SelectedIndex).Loop) break;
                }
                if (ExistsDestinationPage(DestinationSelector.SelectedIndex, index)) break;
            }
            if (index != DestinationPageSelector.SelectedIndex && !(DestinationPageLoop.IsChecked == false && index == 0))
            {
                DestinationPageSelector.SelectedIndex = index;
                DestinationPlay();
            }
            else DestinationStop();
        }  
        #endregion
        #endregion

        #region Stops
        #region Manipulation
        private bool ExistsStop(int message)
        {
            if (message != -1 && Stops.Messages[message] != null) return true;
            return false;
        }

        private bool ExistsStopPage(int message, int page)
        {
            if (ExistsStop(message) && page != -1 && Stops.Messages[message].Pages[page] != null) return true;
            return false;
        }

        private MessageStorage GetStop(int message)
        {
            return Stops.Messages[message];
        }

        private void SetStop(int message, MessageStorage storage)
        {
            CreateStop(message);
            Stops.Messages[message] = storage;
        }

        private PageStorage GetStopPage(int message, int page)
        {
            if (ExistsStop(message)) return Stops.Messages[message].Pages[page];
            return null;
        }

        private void SetStopPage(int message, int page, PageStorage storage)
        {
            CreateStopPage(message, page);
            Stops.Messages[message].Pages[page] = storage;
            UpdateStopPageList();
        }

        private void UpdateStop()
        {
            if (ExistsStop(StopSelector.SelectedIndex))
            {
                StopSelector.Background = ItemExists;
                StopPageLoop.IsChecked = GetStop(StopSelector.SelectedIndex).Loop;
                int tmp = 0;
                for (int i = 0; i < StopPageSelector.Items.Count; ++i) if (ExistsStopPage(StopSelector.SelectedIndex, i)) ++tmp;
                StopPageCount.Text = tmp.ToString();
            }
            else
            {
                StopSelector.Background = Brushes.White;
                StopPageLoop.IsChecked = false;
                StopPageCount.Text = "0";
            }
            UpdateStopText(StopSelector.SelectedIndex);
        }

        private void UpdateStopText(int message)
        {
            if (ExistsStop(message))
            {
                int page;
                for (page = 0; page < StopPageSelector.Items.Count && !ExistsStopPage(message, page); ++page) { }
                ((ComboBoxItem)StopSelector.Items[message]).Content = message + " Stop - " + Stops.Messages[message].Pages[page].Strings[0].Text;
            }
            else if (message >= 0) ((ComboBoxItem)StopSelector.Items[message]).Content = message + " Stop (no content)";
        }

        private void UpdateStopPage()
        {
            if (ExistsStopPage(StopSelector.SelectedIndex, StopPageSelector.SelectedIndex)) StopPageSelector.Background = ItemExists;
            else StopPageSelector.Background = Brushes.White;
            UpdateStop();
            UpdateStopPageText(StopSelector.SelectedIndex, StopPageSelector.SelectedIndex);
        }

        private void UpdateStopPageText(int message, int page)
        {
            if (ExistsStopPage(message, page)) ((ComboBoxItem)StopPageSelector.Items[page]).Content = (page + 1) + " page - " + Stops.Messages[message].Pages[page].Strings[0].Text;
            else if (page >= 0) ((ComboBoxItem)StopPageSelector.Items[page]).Content = (page + 1) + " page (no content)";
        }

        private void UpdateStopPageList()
        {
            bool tmp = true;
            for (int i = 0; i < StopPageSelector.Items.Count; ++i)
            {
                if (ExistsStopPage(StopSelector.SelectedIndex, i))
                {
                    ((ComboBoxItem)StopPageSelector.Items[i]).Background = ItemExists;
                    tmp = false;
                }
                else ((ComboBoxItem)StopPageSelector.Items[i]).Background = Brushes.White;
                UpdateStopPageText(StopSelector.SelectedIndex, i);
            }
            if (tmp) RemoveStop(StopSelector.SelectedIndex);
            UpdateStopPage();
        }

        private bool CreateStop(int message)
        {
            if (ExistsStop(message)) return false;
            Stops.Messages[message] = new MessageStorage(false, Settings.Default.MessagePageCount);
            ((ComboBoxItem)StopSelector.Items[message]).Background = ItemExists;
            //UpdateStop();
            return true;
        }

        private bool CreateStopPage(int message, int page)
        {
            if (ExistsStopPage(message, page)) return false;
            CreateStop(message);
            Stops.Messages[message].Pages[page] = new PageStorage(1, 1);
            //UpdateStopPageList();
            return true;
        }

        private bool RemoveStop(int message)
        {
            if (!ExistsStop(message)) return false;
            Stops.Messages[message] = null;
            ((ComboBoxItem)StopSelector.Items[message]).Background = Brushes.White;
            UpdateStop();
            return true;
        }

        private bool RemoveStopPage(int message, int page)
        {
            if (!ExistsStopPage(message, page)) return false;
            Stops.Messages[message].Pages[page] = null;
            UpdateStopPageList();
            return true;
        }
        #endregion

        #region Events
        private void EditStop(object sender, RoutedEventArgs e)
        {
            StopStop();
            PageEditor pe = new PageEditor(ExistsStopPage(StopSelector.SelectedIndex, StopPageSelector.SelectedIndex) ? GetStopPage(StopSelector.SelectedIndex, StopPageSelector.SelectedIndex) : null, true, new Int32Rect(0, 0, Settings.Default.DisplayWidth, Settings.Default.DisplayHeight), StopSelector.SelectedIndex, "stop", StopPageSelector.SelectedIndex, StopPageSelector.Items.Count);

            if (pe.ShowDialog() == true)
            {
                SetStopPage(StopSelector.SelectedIndex, StopPageSelector.SelectedIndex, pe.Page);
                RefreshPreview();
            }
        }

        private void ClearStop(object sender, RoutedEventArgs e)
        {
            StopStop();
            if (ExistsStopPage(StopSelector.SelectedIndex, StopPageSelector.SelectedIndex) && MessageBox.Show("Entry will be deleted permamently. Continue?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                RemoveStopPage(StopSelector.SelectedIndex, StopPageSelector.SelectedIndex);
                RefreshPreview();
            }
        }

        private void StopChanged(object sender, SelectionChangedEventArgs e)
        {
            StopStop();
            UpdateStopPageList();
            StopPageSelector.SelectedIndex = 0;
            RefreshPreview();
        }

        private void StopPageChanged(object sender, SelectionChangedEventArgs e)
        {
            StopStop();
            UpdateStopPage();
            RefreshPreview();
        }

        private void StopPageLoopChecked(object sender, RoutedEventArgs e)
        {
            if (ExistsStop(StopSelector.SelectedIndex)) Stops.Messages[StopSelector.SelectedIndex].Loop = (StopPageLoop.IsChecked == true);
            else StopPageLoop.IsChecked = false;
            RefreshPreview();
        }

        private void StopPlayClicked(object sender, RoutedEventArgs e)
        {
            if (StopTimer.Enabled) StopStop();
            else StopPlay();
        }

        private void StopPlay()
        {
            StopTimer.Enabled = false;
            if (ExistsStop(StopSelector.SelectedIndex))
            {
                if (ExistsStopPage(StopSelector.SelectedIndex, StopPageSelector.SelectedIndex)) StopTimer.Interval = GetStopPage(StopSelector.SelectedIndex, StopPageSelector.SelectedIndex).Time * 1000;
                else StopTimer.Interval = 1;

                StopPlayButton.Content = "";
                StopTimer.Enabled = true;
            }
        }

        private void StopStop()
        {
            StopPlayButton.Content = "";
            StopTimer.Enabled = false;
        }

        void StopTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TimerTick(StopTimerTick));
        }

        private void StopTimerTick()
        {
            int index = StopPageSelector.SelectedIndex + 1;
            for (int i = 0; i < StopPageSelector.Items.Count; ++i, ++index)
            {
                if (index == StopPageSelector.Items.Count)
                {
                    index = 0;
                    if (!GetStop(StopSelector.SelectedIndex).Loop) break;
                }
                if (ExistsStopPage(StopSelector.SelectedIndex, index)) break;
            }
            if (index != StopPageSelector.SelectedIndex && !(StopPageLoop.IsChecked == false && index == 0))
            {
                StopPageSelector.SelectedIndex = index;
                StopPlay();
            }
            else StopStop();
        }
        #endregion
        #endregion
    }
}
