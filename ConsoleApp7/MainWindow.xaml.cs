using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ConsoleApp7.Data;
using ConsoleApp7.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp7;

public partial class MainWindow : Window
{
    private ObservableCollection<BookViewModel> _allBooks = new();
    private string _searchText = "";
    private int? _filterAuthorId;
    private int? _filterGenreId;

    public MainWindow()
    {
        InitializeComponent();
        LoadFilters();
        LoadBooks();
    }

    private void LoadFilters()
    {
        using var db = DbContextFactory.Create();
        GenreFilterComboBox.Items.Clear();
        GenreFilterComboBox.Items.Add(new ComboBoxItem { Content = "(Все жанры)", Tag = (int?)null });
        foreach (var g in db.Genres.OrderBy(x => x.Name))
            GenreFilterComboBox.Items.Add(new ComboBoxItem { Content = g.Name, Tag = (int?)g.Id });
        GenreFilterComboBox.SelectedIndex = 0;

        AuthorFilterComboBox.Items.Clear();
        AuthorFilterComboBox.Items.Add(new ComboBoxItem { Content = "(Все авторы)", Tag = (int?)null });
        foreach (var a in db.Authors.OrderBy(x => x.LastName))
            AuthorFilterComboBox.Items.Add(new ComboBoxItem { Content = a.FullName, Tag = (int?)a.Id });
        AuthorFilterComboBox.SelectedIndex = 0;
    }

    private void LoadBooks()
    {
        using var db = DbContextFactory.Create();
        var query = db.Books.Include(b => b.Author).Include(b => b.Genre).AsQueryable();

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var term = _searchText.Trim().ToLower();
            query = query.Where(b => b.Title.ToLower().Contains(term));
        }
        if (_filterAuthorId.HasValue)
            query = query.Where(b => b.AuthorId == _filterAuthorId.Value);
        if (_filterGenreId.HasValue)
            query = query.Where(b => b.GenreId == _filterGenreId.Value);

        var list = query.OrderBy(b => b.Title)
            .Select(b => new BookViewModel
            {
                Id = b.Id,
                Title = b.Title,
                AuthorName = b.Author.FullName,
                PublishYear = b.PublishYear,
                ISBN = b.ISBN,
                GenreName = b.Genre.Name,
                QuantityInStock = b.QuantityInStock
            })
            .ToList();

        _allBooks.Clear();
        foreach (var item in list)
            _allBooks.Add(item);
        BooksDataGrid.ItemsSource = null;
        BooksDataGrid.ItemsSource = _allBooks;
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = SearchTextBox.Text ?? "";
        LoadBooks();
    }

    private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GenreFilterComboBox.SelectedItem is ComboBoxItem gi)
            _filterGenreId = (int?)gi.Tag;
        if (AuthorFilterComboBox.SelectedItem is ComboBoxItem ai)
            _filterAuthorId = (int?)ai.Tag;
        LoadBooks();
    }

    private void ResetFilters_Click(object sender, RoutedEventArgs e)
    {
        SearchTextBox.Clear();
        GenreFilterComboBox.SelectedIndex = 0;
        AuthorFilterComboBox.SelectedIndex = 0;
        _searchText = "";
        _filterAuthorId = null;
        _filterGenreId = null;
        LoadBooks();
    }

    private void BooksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var hasSelection = BooksDataGrid.SelectedItem != null;
        EditBookButton.IsEnabled = hasSelection;
        DeleteBookButton.IsEnabled = hasSelection;
    }

    private void AddBook_Click(object sender, RoutedEventArgs e)
    {
        var wnd = new AddEditBookWindow(null);
        if (wnd.ShowDialog() == true)
        {
            LoadFilters();
            LoadBooks();
        }
    }

    private void EditBook_Click(object sender, RoutedEventArgs e)
    {
        if (BooksDataGrid.SelectedItem is not BookViewModel vm) return;
        var wnd = new AddEditBookWindow(vm.Id);
        if (wnd.ShowDialog() == true)
        {
            LoadFilters();
            LoadBooks();
        }
    }

    private void DeleteBook_Click(object sender, RoutedEventArgs e)
    {
        if (BooksDataGrid.SelectedItem is not BookViewModel vm) return;
        if (MessageBox.Show($"Удалить книгу «{vm.Title}»?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        using var db = DbContextFactory.Create();
        var book = db.Books.Find(vm.Id);
        if (book != null)
        {
            db.Books.Remove(book);
            db.SaveChanges();
        }
        LoadBooks();
    }

    private void ManageAuthors_Click(object sender, RoutedEventArgs e)
    {
        var wnd = new AuthorsWindow();
        wnd.ShowDialog();
        LoadFilters();
        LoadBooks();
    }

    private void ManageGenres_Click(object sender, RoutedEventArgs e)
    {
        var wnd = new GenresWindow();
        wnd.ShowDialog();
        LoadFilters();
        LoadBooks();
    }
}
