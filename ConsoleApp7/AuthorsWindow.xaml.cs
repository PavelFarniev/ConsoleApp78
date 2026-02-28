using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ConsoleApp7.Data;
using ConsoleApp7.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp7;

public partial class AuthorsWindow : Window
{
    private ObservableCollection<Author> _authors = new();

    public AuthorsWindow()
    {
        InitializeComponent();
        AuthorsDataGrid.SelectionChanged += (_, _) =>
        {
            var hasSelection = AuthorsDataGrid.SelectedItem != null;
            EditButton.IsEnabled = hasSelection;
            DeleteButton.IsEnabled = hasSelection;
        };
        LoadAuthors();
    }

    private void LoadAuthors()
    {
        using var db = DbContextFactory.Create();
        var list = db.Authors.OrderBy(a => a.LastName).ToList();
        _authors.Clear();
        foreach (var a in list)
            _authors.Add(a);
        AuthorsDataGrid.ItemsSource = null;
        AuthorsDataGrid.ItemsSource = _authors;
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var wnd = new AuthorEditWindow(null);
        if (wnd.ShowDialog() == true)
            LoadAuthors();
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (AuthorsDataGrid.SelectedItem is not Author author) return;
        var wnd = new AuthorEditWindow(author.Id);
        if (wnd.ShowDialog() == true)
            LoadAuthors();
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (AuthorsDataGrid.SelectedItem is not Author author) return;
        using var db = DbContextFactory.Create();
        var count = db.Books.Count(b => b.AuthorId == author.Id);
        if (count > 0)
        {
            MessageBox.Show($"Нельзя удалить автора: с ним связано книг: {count}. Удалите или измените книги сначала.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (MessageBox.Show($"Удалить автора {author.FullName}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        var entity = db.Authors.Find(author.Id);
        if (entity != null)
        {
            db.Authors.Remove(entity);
            db.SaveChanges();
        }
        LoadAuthors();
    }
}
