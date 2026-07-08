using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;
using Chat.Core.DTOs;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7166/") };
        private HubConnection? _connection;
        private int _currentUserId;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            AuthStatusText.Text = "Идёт регистрация";
            var authDto = new AuthDto { Username = UsernameBox.Text, Password = PasswordBox.Password };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", authDto);

                if (response.IsSuccessStatusCode)
                {
                    AuthStatusText.Text = "Регистрация успешна, так что теперь вы можете войти";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    AuthStatusText.Text = $"Ошибка: {error}";
                }
            }
            catch (Exception ex)
            {
                AuthStatusText.Text = $"Ошибка сети: {ex.Message}";
            }
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            AuthStatusText.Text = "Идёт авторизация";
            var authDto = new AuthDto { Username = UsernameBox.Text, Password = PasswordBox.Password };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", authDto);

                if (response.IsSuccessStatusCode)
                {
                    var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    if (authResult != null)
                    {
                        _currentUserId = authResult.UserId;
                        await InitializeSignalR(authResult.Token);

                        AuthPanel.Visibility = Visibility.Collapsed;
                        ChatPanel.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    AuthStatusText.Text = "Неверный логин или пароль";
                }
            }
            catch (Exception ex)
            {
                AuthStatusText.Text = $"Ошибка сети: {ex.Message}";
            }
        }

        private async Task InitializeSignalR(string token)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl($"{_httpClient.BaseAddress}chat", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult((string?)token);
                })
                .WithAutomaticReconnect()
                .Build();

            _connection.On<MessageDto>("ReceiveMessage", message =>
            {
                Dispatcher.Invoke(() =>
                {
                    MessagesList.Items.Add(message);
                });
            });

            _connection.On<int>("MessageDeleted", messageId =>
            {
                Dispatcher.Invoke(() =>
                {
                    var messageToRemove = MessagesList.Items
                        .Cast<MessageDto>()
                        .FirstOrDefault(m => m.Id == messageId);

                    if (messageToRemove != null)
                    {
                        MessagesList.Items.Remove(messageToRemove);
                    }
                });
            });

            await _connection.StartAsync();
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (_connection == null || string.IsNullOrWhiteSpace(MessageBox.Text)) return;

            try
            {
                if (int.TryParse(ReceiverIdBox.Text, out int receiverId))
                {
                    await _connection.InvokeAsync("SendPrivateMessage", receiverId, MessageBox.Text);
                }
                else
                {
                    await _connection.InvokeAsync("SendMessage", MessageBox.Text);
                }

                MessageBox.Clear();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка отправки: {ex.Message}");
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_connection == null || MessagesList.SelectedItem is not MessageDto selectedMessage) return;

            if (selectedMessage.SenderId != _currentUserId)
            {
                System.Windows.MessageBox.Show("Вы можете удалять только свои сообщения");
                return;
            }

            try
            {
                await _connection.InvokeAsync("DeleteMessage", selectedMessage.Id);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }
    }
}