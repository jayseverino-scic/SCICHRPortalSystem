using MySql.Data.MySqlClient;
using Renci.SshNet;
using System.Data;
using System;
using Microsoft.Extensions.Configuration;

namespace SCICHRPortal.API
{
    public class MySqlSshService : IDisposable
    {
        private readonly IConfiguration _config;
        private SshClient _sshClient;
        private ForwardedPortLocal _portForwarded;
        private bool _disposed;

        public MySqlSshService(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection GetConnection()
        {
            try
            {
                // Read SSH settings
                var sshHost = _config["SshSettings:Host"];
                var sshPort = int.Parse(_config["SshSettings:Port"]);
                var sshUser = _config["SshSettings:Username"];
                var sshPass = _config["SshSettings:Password"];

                // Read DB settings
                var dbServer = _config["DatabaseSettings:Server"];
                var dbPort = uint.Parse(_config["DatabaseSettings:Port"]);
                var dbUser = _config["DatabaseSettings:User"];
                var dbPass = _config["DatabaseSettings:Password"];
                var dbName = _config["DatabaseSettings:Database"];

                // Start SSH tunnel
                _sshClient = new SshClient(sshHost, sshPort, sshUser, sshPass);
                _sshClient.Connect();

                if (!_sshClient.IsConnected)
                    throw new Exception("SSH connection failed.");

                // Forward local port to remote MySQL server
                _portForwarded = new ForwardedPortLocal("127.0.0.1", dbPort, dbServer, dbPort);
                _sshClient.AddForwardedPort(_portForwarded);
                _portForwarded.Start();

                // Build MySQL connection string
                var connString = $"Server=127.0.0.1;Port={dbPort};Database={dbName};User ID={dbUser};Password={dbPass};SslMode=Preferred;";

                var connection = new MySqlConnection(connString);
                connection.Open();

                return connection;
            }
            catch (Exception ex)
            {
                throw new Exception("Error establishing SSH/MySQL connection", ex);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _portForwarded?.Stop();
            _sshClient?.Disconnect();
            _sshClient?.Dispose();
            _disposed = true;
        }
    }
}
