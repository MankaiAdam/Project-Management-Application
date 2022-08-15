using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace Project_Management
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// test
    public partial class MainWindow : Window
    {
        //Classes
        private class page
        {
            public string Name { get; set; }
            public List<group> page_groups = new();
            public page(string s, List<group> g)
            {
                Name = s;
                page_groups = g;
            }
        }
        private class group
        {
            public string Name { get; set; }
            public List<task> group_tasks = new();
            public group(string s, List<task> t)
            {
                Name = s;
                group_tasks = t;
            }
        }
        private class task
        {
            public task(string s)
            {
                Name = s;
            }
            public string Name { get; set; }
            public Image Image { get; set; }
            public List<String> Comments { get; set; }
        }

        Button add_btn = new();
        int group_count = 0;
        List<page> page_list = new();
        int selected_page_index = 0;
        bool hovering_over_valid_position = false;
        int task_drop_slot_index = 0;
        int task_drop_group_index = 0;
        Border task_clone_border = new();
        bool dragging = false;
        bool pressed = false;
        int selected_task_group_index = 0;
        int selected_task_index = 0;
        Border dragged_task_border = new();
        StackPanel dragged_task_panel = new();
        StackPanel dragged_from_Group_panel = new();
        Point mouse_click_pos = new();
        Point mouse_relative_pos = new();
        TextBox page_title;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            panel.Children.Clear();
            TextBox page_title_temp = new()
            {
                Width = 120,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(15, 5, 0, 0),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
            };
            page_title_temp.TextChanged += new TextChangedEventHandler(page_title_TextChanged);
            stackPanel.Children.Insert(0, page_title_temp);
            page_title = (TextBox)stackPanel.Children[0];
            Fill_Page_Panel();
        }

        private void save_pages(List<page> page_list)
        {
            using (StreamWriter myFile = new StreamWriter(@"D:\path.json"))
            {
                myFile.WriteLine(JsonConvert.SerializeObject(page_list));
            }
        }



        void Fill_Page_Panel()
        {
            using (StreamReader file = new StreamReader(@"D:\path.json"))
            {
                page_list = JsonConvert.DeserializeObject<List<page>>(file.ReadToEnd());
                file.Close();
            }
            for (int i = 0; i < page_list.Count; i++)
            {
                Button page_btn = new();
                page_btn.Content = page_list[i].Name;
                page_btn.Tag = i;
                page_btn.Style = (Style)Application.Current.Resources["page_button"];
                page_btn.Click += new RoutedEventHandler(this.page_btn_Click);
                page_panel.Children.Insert(i + 1, page_btn);
            }
        }

        private void add_page_Btn_Click(object sender, RoutedEventArgs e)
        {
            List<task> task_list = new();
            List<group> group_list = new();
            group to_do = new group("To Do", task_list);
            group doing = new group("Doing", task_list);
            group done = new group("Done", task_list);
            group_list.Add(to_do);
            group_list.Add(doing);
            group_list.Add(done);
            page p = new("Untitled " + page_list.Count, group_list);
            page_list.Add(p);
            //Add Page Button
            Button page_btn = new();
            page_btn.Content = "Untitled " + (page_list.Count - 1);
            page_btn.Tag = page_list.Count - 1;
            page_btn.Style = (Style)Application.Current.Resources["page_button"];
            page_btn.Click += new RoutedEventHandler(this.page_btn_Click);
            page_panel.Children.Insert(page_list.Count, page_btn);
            save_pages(page_list);
        }

        private void page_btn_Click(object sender, RoutedEventArgs e)
        {
            group_count = 0;
            panel.Children.Clear();
            panel.Width = 20;
            Button page_btn = (Button)sender;
            page page = page_list[(int)page_btn.Tag];
            selected_page_index = (int)page_btn.Tag;
            page_title.Text = page.Name;
            for (int i = 0; i < page.page_groups.Count; i++)
            {
                Add_Group(page.page_groups[i]);
            }
            add_btn = new()
            {
                Style = (Style)Application.Current.Resources["add_button"],
                Margin = new Thickness(10 + (250 * group_count), 10, 0, 0),
                Name = "Add_btn",
            };
            add_btn.Click += new RoutedEventHandler(this.add_group_btn_Click);
            panel.Children.Add(add_btn);
            panel.Width = panel.Width + 250;
        }

        private void add_group_btn_Click(object sender, RoutedEventArgs e)
        {
            Button add_btn = (Button)sender;
            List<task> task_list = new();
            group group = new group("task" + group_count, task_list);
            Add_Group(group);
            add_btn.Margin = new Thickness(10 + (250 * group_count), 10, 0, 0);
            page_list[selected_page_index].page_groups.Add(group);
            save_pages(page_list);
        }

        void Add_Group(group group)
        {
            Border group_border = new()
            {
                Name = "group_panel",
                Style = (Style)Application.Current.Resources["group_border"],
                Margin = new Thickness(10 + (250 * group_count), 10, 0, 0),
            };

            StackPanel group_panel = new()
            {
                Orientation = Orientation.Vertical,
                Tag = group_count
            };

            StackPanel group_name_panel = new()
            {
                Orientation = Orientation.Horizontal
            };

            Button delete_group_btn = new()
            {
                Style = (Style)Application.Current.Resources["delete_group_button"],
                Tag = group_count,
            };
            delete_group_btn.Click += new RoutedEventHandler(this.delete_group_btn_Click);

            TextBox group_name = new()
            {
                Text = group.Name,
                Tag = group_count,
                Style = (Style)Application.Current.Resources["group_name"]
            };
            group_name.TextChanged += new TextChangedEventHandler(this.group_name_Changed);
            group_name_panel.Children.Add(group_name);
            group_name_panel.Children.Add(delete_group_btn);
            group_panel.Children.Add(group_name_panel);

            Button add_task_btn = new()
            {
                Tag = group_count,
                Style = (Style)Application.Current.Resources["add_task_button"],
            };

            add_task_btn.Click += new RoutedEventHandler(this.add_task_btn_Click);
            group_panel.Children.Add(add_task_btn);

            add_seperator(group_panel, 0, 1);

            for (int i = 0; i < group.group_tasks.Count; i++)
            {
                add_task(group_panel, group.group_tasks[i].Name, i, group_panel.Children.Count - 1);
            }

            group_border.Child = group_panel;

            panel.Children.Insert(group_count, group_border);
            panel.Width = panel.Width + 250;

            group_count++;

        }

        private void delete_group_btn_Click(object sender, RoutedEventArgs e)
        {
            Button delete_group_btn = (Button)sender;
            Debug.WriteLine(delete_group_btn.Tag);
            Debug.WriteLine("nb of grps: " + group_count);
            panel.Children.RemoveAt((int)delete_group_btn.Tag);
            panel.Width = panel.Width - 250;
            for (int i = (int)delete_group_btn.Tag; i < panel.Children.Count - 1; i++)
            {
                StackPanel group_panel = (StackPanel)((Border)panel.Children[i]).Child;
                Border group_border = (Border)group_panel.Parent;
                group_border.Margin = new Thickness(group_border.Margin.Left - 250, 10, 0, 0);
                group_panel.Tag = (int)group_panel.Tag - 1;
                TextBox group_name = (TextBox)((StackPanel)group_panel.Children[0]).Children[0];
                Button delete_task = (Button)((StackPanel)group_panel.Children[0]).Children[1];
                Button add_task_btn = (Button)group_panel.Children[group_panel.Children.Count - 1];
                group_name.Tag = (int)group_name.Tag - 1;
                delete_task.Tag = (int)delete_task.Tag - 1;
                add_task_btn.Tag = (int)add_task_btn.Tag - 1;
            }
            add_btn.Margin = new Thickness(add_btn.Margin.Left - 250, 10, 0, 0);
            page_list[selected_page_index].page_groups.RemoveAt((int)delete_group_btn.Tag);
            group_count--;
            save_pages(page_list);
        }

        private void add_task_btn_Click(object sender, RoutedEventArgs e)
        {
            Button add_task_btn = (Button)sender;
            StackPanel group_panel = (StackPanel)add_task_btn.Parent;

            add_task(group_panel, "task", (int)((group_panel.Children.Count - 3) / 2), group_panel.Children.Count - 1);

            task task = new("task");
            page_list[selected_page_index].page_groups[(int)add_task_btn.Tag].group_tasks.Add(task);
            save_pages(page_list);
        }

        void add_task(StackPanel group_panel, string name, int tag, int position)
        {
            Border task_border = new()
            {
                Style = (Style)Application.Current.Resources["task_border"],
                Tag = tag,
            };
            task_border.MouseLeftButtonUp += new MouseButtonEventHandler(this.task_released);
            task_border.MouseMove += new MouseEventHandler(this.task_hovered);
            task_border.MouseLeave += new MouseEventHandler(this.task_left);

            StackPanel task_panel = new()
            {
                Orientation = Orientation.Horizontal,
                Style = (Style)Application.Current.Resources["task_panel"]
            };

            Button task_box = new()
            {
                Content = name,
                Tag = tag,
                Style = (Style)Application.Current.Resources["task"],
                IsHitTestVisible = false,
                IsEnabled = true
            };
            /*task_box.MouseMove += new MouseEventHandler(this.task_hovered);
            task_box.MouseLeave += new MouseEventHandler(this.task_left);*/
            //task_box.TextChanged += new TextChangedEventHandler(this.task_name_Changed);

            Uri resourceUri = new Uri("Images/delete8.png", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            var brush = new ImageBrush();
            brush.Stretch = Stretch.Fill;
            brush.ImageSource = temp;

            Button delete_task_btn = new()
            {
                Style = (Style)Application.Current.Resources["delete_task_button"],
                Tag = tag,
                Background = brush
            };
            delete_task_btn.Click += new RoutedEventHandler(this.delete_task_btn_Click);

            task_panel.Children.Add(task_box);
            task_panel.Children.Add(delete_task_btn);
            task_border.Child = task_panel;
            group_panel.Children.Insert(position, task_border);

            add_seperator(group_panel, tag + 1, position + 1);
        }

        void add_seperator(StackPanel group_panel, int tag, int position)
        {
            StackPanel task_seperator = new()
            {
                Style = (Style)Application.Current.Resources["task_separator"],
                Tag = tag,
            };
            Border task_seperator_border = new()
            {
                Child = task_seperator,
                Style = (Style)Application.Current.Resources["task_separator_border"],
                Tag = tag,
            };
            task_seperator_border.MouseEnter += new MouseEventHandler(this.task_seperator_hovered);
            task_seperator_border.MouseLeave += new MouseEventHandler(this.task_seperator_left);
            group_panel.Children.Insert(position, task_seperator_border);
        }
        private void delete_task_btn_Click(object sender, RoutedEventArgs e)
        {
            Button delete_task_btn = (Button)sender;
            delete_task(delete_task_btn);
        }
        private void delete_task_view_btn_Click(object sender, RoutedEventArgs e)
        {

            Border selected_task_border = (Border)((StackPanel)((Border)panel.Children[selected_task_group_index]).Child).Children[(selected_task_index * 2) + 2];
            Button delete_task_btn = (Button)((StackPanel)selected_task_border.Child).Children[1];
            delete_task(delete_task_btn);
            panel.Children.RemoveAt(panel.Children.Count - 1);
        }

        void delete_task(Button delete_task_btn)
        {
            Panel group_panel = (Panel)((Border)((Panel)delete_task_btn.Parent).Parent).Parent;
            group_panel.Children[((int)delete_task_btn.Tag * 2) + 2].MouseLeave -= new MouseEventHandler(this.task_left);
            Debug.WriteLine("deleted " + (int)delete_task_btn.Tag);
            group_panel.Children.RemoveAt(((int)delete_task_btn.Tag * 2) + 2);
            group_panel.Children.RemoveAt(((int)delete_task_btn.Tag * 2) + 2);
            for (int i = ((int)delete_task_btn.Tag * 2) + 2; i < group_panel.Children.Count - 1; i += 2)
            {
                Border task_seperator_border = (Border)group_panel.Children[i + 1];
                Border task_border = (Border)group_panel.Children[i];
                StackPanel task_panel = (StackPanel)task_border.Child;
                Button task_box = (Button)task_panel.Children[0];
                Button delete_task = (Button)task_panel.Children[1];
                task_seperator_border.Tag = (int)task_seperator_border.Tag - 1;
                task_border.Tag = (int)task_border.Tag - 1;
                task_box.Tag = (int)task_box.Tag - 1;
                delete_task.Tag = (int)delete_task.Tag - 1;
            }
            page_list[selected_page_index].page_groups[(int)group_panel.Tag].group_tasks.RemoveAt((int)delete_task_btn.Tag);
            save_pages(page_list);
        }

        private void page_title_TextChanged(object sender, TextChangedEventArgs e)
        {
            page_list[selected_page_index].Name = page_title.Text;
            Button page_menu_title = (Button)page_panel.Children[selected_page_index + 1];
            page_menu_title.Content = page_title.Text;
            save_pages(page_list);
        }

        private void group_name_Changed(object sender, TextChangedEventArgs e)
        {
            TextBox group_name = (TextBox)sender;
            Debug.WriteLine((int)group_name.Tag);
            page_list[selected_page_index].page_groups[(int)group_name.Tag].Name = group_name.Text;
            save_pages(page_list);
        }

        private void task_name_Changed(object sender, TextChangedEventArgs e)
        {
            Border selected_task_border = (Border)((StackPanel)((Border)panel.Children[selected_task_group_index]).Child).Children[(selected_task_index * 2) + 2];
            Button task_name = (Button)((StackPanel)selected_task_border.Child).Children[0];
            TextBox task_name_view = (TextBox)sender;
            task_name.Content = task_name_view.Text;
            page_list[selected_page_index].page_groups[selected_task_group_index].group_tasks[selected_task_index].Name = task_name_view.Text;
            save_pages(page_list);
        }

        private void task_released(object sender, MouseButtonEventArgs e)
        {
            if (!dragging)
            {
                Debug.WriteLine("pressed");
                Border task_border = (Border)sender;
                StackPanel task_group_panel = (StackPanel)task_border.Parent;
                selected_task_group_index = (int)task_group_panel.Tag;
                selected_task_index = (int)task_border.Tag;
                Border task_view_border = new()
                {
                    Style = (Style)Application.Current.Resources["task_view_border"],
                };


                StackPanel task_view_panel = new()
                {
                    Orientation = Orientation.Vertical,
                    Style = (Style)Application.Current.Resources["task_view_panel"]
                };

                TextBox task_box = new()
                {
                    Text = (string)((Button)((StackPanel)task_border.Child).Children[0]).Content,
                    Tag = task_border.Tag,
                    Style = (Style)Application.Current.Resources["task_view"],
                };
                task_box.TextChanged += new TextChangedEventHandler(this.task_name_Changed);

                Uri resourceUri = new Uri("Images/delete5.png", UriKind.Relative);
                StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

                BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
                var brush = new ImageBrush();
                brush.Stretch = Stretch.Fill;
                brush.ImageSource = temp;

                Button delete_task_btn = new()
                {
                    Style = (Style)Application.Current.Resources["delete_task_view_button"],
                    Tag = task_border.Tag,
                    Background = brush
                };
                delete_task_btn.Click += new RoutedEventHandler(this.delete_task_view_btn_Click);
                Button exit_task_view_btn = new()
                {
                    Background = new SolidColorBrush(Colors.Transparent),
                    Width = 1000,
                    Height = 1000
                };
                task_view_panel.Children.Add(task_box);
                task_view_panel.Children.Add(delete_task_btn);
                task_view_border.Child = task_view_panel;
                panel.Children.Add(task_view_border);

                Point window_dimensions = new(window.Width, window.Height);
                Canvas.SetLeft(task_view_border, (int)((window_dimensions.X - task_view_border.Width) / 2) - page_panel.Width);
                Canvas.SetTop(task_view_border, (int)((window_dimensions.Y - task_view_border.Height) / 2) - page_title.Height);

                Debug.WriteLine(page_title.Height);
            }
        }

        private void task_hovered(object sender, MouseEventArgs e)
        {

            if (!pressed)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    pressed = true;
                    mouse_click_pos = e.GetPosition(panel);
                    //Debug.WriteLine("Start Drag");
                    dragged_task_border = (Border)sender;
                    dragged_task_panel = (StackPanel)dragged_task_border.Child;
                    dragged_from_Group_panel = (StackPanel)dragged_task_border.Parent;
                    mouse_relative_pos = e.GetPosition(dragged_task_border);

                    //Dispatcher.BeginInvoke(new Action(() => DragDrop.DoDragDrop(task_border, task_border, DragDropEffects.Move)));
                }
            }
            else
            {
                Border task_border = (Border)sender;
                StackPanel group_panel = (StackPanel)task_border.Parent;
                StackPanel task_seperator = new();
                task_drop_group_index = (int)group_panel.Tag;
                if (!((int)task_border.Tag == (int)dragged_task_border.Tag && (int)dragged_from_Group_panel.Tag == (int)group_panel.Tag))
                {

                    if (e.GetPosition(task_border).Y > 14)
                    {
                        if (!((int)task_border.Tag == (int)dragged_task_border.Tag - 1 && (int)dragged_from_Group_panel.Tag == (int)group_panel.Tag))
                        {
                            hovering_over_valid_position = true;
                            task_seperator = (StackPanel)((Border)group_panel.Children[((int)task_border.Tag * 2) + 3]).Child;
                            task_seperator.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0));
                            task_seperator = (StackPanel)((Border)group_panel.Children[((int)task_border.Tag * 2) + 1]).Child;
                            task_seperator.Background = new SolidColorBrush(Colors.Transparent);
                            task_drop_slot_index = ((int)task_border.Tag * 2) + 3;
                        }
                    }
                    else
                    {
                        if (!((int)task_border.Tag == (int)dragged_task_border.Tag + 1 && (int)dragged_from_Group_panel.Tag == (int)group_panel.Tag))
                        {
                            hovering_over_valid_position = true;
                            task_seperator = (StackPanel)((Border)group_panel.Children[((int)task_border.Tag * 2) + 1]).Child;
                            task_seperator.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0));
                            task_seperator = (StackPanel)((Border)group_panel.Children[((int)task_border.Tag * 2) + 3]).Child;
                            task_seperator.Background = new SolidColorBrush(Colors.Transparent);
                            task_drop_slot_index = ((int)task_border.Tag * 2) + 1;
                        }
                    }
                }
            }
        }
        private void canvas_drag_over(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (pressed)
                {
                    Point mouse_pos = e.GetPosition(panel);
                    //Debug.WriteLine(Math.Abs(mouse_pos.Y - mouse_click_pos.Y));

                    if (!dragging)
                    {
                        if (Math.Abs(mouse_pos.Y - mouse_click_pos.Y) > 10 || Math.Abs(mouse_pos.X - mouse_click_pos.X) > 10)
                        {
                            dragging = true;
                            string name = (string)((Button)(((StackPanel)dragged_task_border.Child).Children[0])).Content;
                            dragged_task_border.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0));
                            dragged_task_panel.Visibility = Visibility.Hidden;

                            task_clone_border = new Border();
                            task_clone_border.Style = (Style)Application.Current.Resources["task_clone_border"];

                            StackPanel task_clone_panel = new();
                            task_clone_panel.Orientation = Orientation.Horizontal;
                            task_clone_panel.Style = (Style)Application.Current.Resources["task_panel"];
                            Button task_clone_box = new()
                            {
                                Content = name,
                                Style = (Style)Application.Current.Resources["task"],
                                IsEnabled = false
                            };

                            task_clone_panel.Children.Add(task_clone_box);
                            task_clone_border.Child = task_clone_panel;
                            panel.Children.Add(task_clone_border);
                            panel.Children[panel.Children.Count - 1].IsHitTestVisible = false;
                            Canvas.SetLeft(panel.Children[panel.Children.Count - 1], mouse_pos.X - mouse_relative_pos.X);
                            Canvas.SetTop(panel.Children[panel.Children.Count - 1], mouse_pos.Y - mouse_relative_pos.Y);
                        }
                    }
                    else
                    {
                        //Debug.WriteLine("dragging");
                        Canvas.SetLeft(panel.Children[panel.Children.Count - 1], mouse_pos.X - mouse_relative_pos.X);
                        Canvas.SetTop(panel.Children[panel.Children.Count - 1], mouse_pos.Y - mouse_relative_pos.Y);
                    }
                }
            }
            else
            {
                pressed = false;
                dragging = false;
            }


        }

        private void canvas_pressed(object sender, MouseButtonEventArgs e)
        {

        }

        private void task_left(object sender, MouseEventArgs e)
        {
            try
            {
                hovering_over_valid_position = false;
                Border task_border = (Border)sender;
                StackPanel group_panel = (StackPanel)task_border.Parent;
                StackPanel task_seperator = new();
                //Debug.WriteLine(group_panel.Children.Count);
                //Debug.WriteLine((int)task_border.Tag);
                task_seperator = (StackPanel)((Border)group_panel.Children[((int)task_border.Tag * 2) + 3]).Child;
                task_seperator.Background = new SolidColorBrush(Colors.Transparent);
                task_seperator = (StackPanel)((Border)group_panel.Children[((int)task_border.Tag * 2) + 1]).Child;
                task_seperator.Background = new SolidColorBrush(Colors.Transparent);
            }
            catch (Exception ex) { }
        }
        private void task_seperator_hovered(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                StackPanel task_seperator = (StackPanel)((Border)sender).Child;
                StackPanel group_panel = (StackPanel)((Border)sender).Parent;
                Debug.WriteLine(task_seperator.Tag);
                Debug.WriteLine(dragged_task_border.Tag);
                if (!(((int)task_seperator.Tag == (int)dragged_task_border.Tag || (int)task_seperator.Tag == (int)dragged_task_border.Tag + 1) && (int)dragged_from_Group_panel.Tag == (int)group_panel.Tag))
                {
                    hovering_over_valid_position = true;
                    task_seperator.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0));
                    task_drop_group_index = (int)group_panel.Tag;
                    task_drop_slot_index = ((int)((Border)sender).Tag * 2) + 1;
                }
            }
        }
        private void task_seperator_left(object sender, MouseEventArgs e)
        {
            hovering_over_valid_position = false;
            Border task_seperator_border = (Border)sender;
            StackPanel group_panel = (StackPanel)task_seperator_border.Parent;
            //Debug.WriteLine(group_panel.Children.Count);
            //Debug.WriteLine((int)task_seperator_border.Tag);
            StackPanel task_seperator = (StackPanel)task_seperator_border.Child;
            task_seperator.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dragging)
            {
                dragging = false;
                panel.Children.RemoveAt(panel.Children.Count - 1);
                dragged_task_border.Background = new SolidColorBrush(Colors.White);
                dragged_task_panel.Visibility = Visibility.Visible;

                if (hovering_over_valid_position)
                {
                    Debug.WriteLine("group " + task_drop_group_index);
                    Debug.WriteLine("Slot " + task_drop_slot_index);

                    string name = (string)((Button)dragged_task_panel.Children[0]).Content;
                    int task_new_index = (int)((task_drop_slot_index - 1) / 2);
                    StackPanel group_panel = (StackPanel)((Border)panel.Children[task_drop_group_index]).Child;
                    add_task(group_panel, name, task_new_index, task_drop_slot_index + 1);

                    //change tha tags of all the next tasks
                    for (int i = task_drop_slot_index + 3; i < group_panel.Children.Count - 1; i += 2)
                    {
                        Border task_seperator_border = (Border)group_panel.Children[i + 1];
                        Border task_border = (Border)group_panel.Children[i];
                        StackPanel task_panel = (StackPanel)task_border.Child;
                        Button task_box = (Button)task_panel.Children[0];
                        Button delete_task = (Button)task_panel.Children[1];
                        task_seperator_border.Tag = (int)task_seperator_border.Tag + 1;
                        task_border.Tag = (int)task_border.Tag + 1;
                        task_box.Tag = (int)task_box.Tag + 1;
                        delete_task.Tag = (int)delete_task.Tag + 1;
                    }

                    //delete the original task
                    delete_task((Button)dragged_task_panel.Children[1]);
                    //StackPanel dragged_task_group_panel = (StackPanel)dragged_task_border.Parent;
                    //dragged_task_group_panel.Children.Remove(dragged_task_border);

                    page_list[selected_page_index].page_groups[task_drop_group_index].group_tasks.Insert(task_new_index, new task(name));
                    save_pages(page_list);
                }
            }
        }
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            if (e.Delta < 0)
            {
                scrollViewer.LineRight();
            }
            else
            {
                scrollViewer.LineLeft();
            }
            e.Handled = true;
        }

        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            scroll_viewer.Width = window.Width - 180;
            scroll_viewer.Height = window.Height;
        }
    }
}
