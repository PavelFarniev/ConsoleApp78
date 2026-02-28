using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ConsoleApp7.Data;
using ConsoleApp7.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp7;

public partial class GenresWindow : Window
{
    private ObservableCollection<Genre> _genres = new();

    public GenresWindow()
    {
        InitializeComponent();
        GenresDataGrid.SelectionChanged += (_, _) =>
        {
            var hasSelection = GenresDataGrid.SelectedItem != null;
            EditButton.IsEnabled = hasSelection;
            DeleteButton.IsEnabled = hasSelection;
        };
        LoadGenres();
    }

    private void LoadGenres()
    {
        using var db = DbContextFactory.Create();
        var list = db.Genres.OrderBy(g => g.Name).ToList();
        _genres.Clear();
        foreach (var g in list)
            _genres.Add(g);
        GenresDataGrid.ItemsSource = null;
        GenresDataGrid.ItemsSource = _genres;
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var wnd = new GenreEditWindow(null);
        if (wnd.ShowDialog() == true)
            LoadGenres();
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (GenresDataGrid.SelectedItem is not Genre genre) return;
        var wnd = new GenreEditWindow(genre.Id);
        if (wnd.ShowDialog() == true)
            LoadGenres();
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (GenresDataGrid.SelectedItem is not Genre genre) return;
        using var db = DbContextFactory.Create();
        var count = db.Books.Count(b => b.GenreId == genre.Id);
        if (count > 0)
        {
            MessageBox.Show($"Нельзя удалить жанр: с ним связано книг: {count}. Удалите или измените книги сначала.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (MessageBox.Show($"Удалить жанр «{genre.Name}»?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        var entity = db.Genres.Find(genre.Id);
        if (entity != null)
        {
            db.Genres.Remove(entity);
            db.SaveChanges();
        }
        LoadGenres();
    }
}
