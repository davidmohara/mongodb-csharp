using System;
using System.IO;
using MongoDB.Driver.Protocol;

namespace MongoDB.Driver.Connections
{
    /// <summary>
    /// Connection is a managment unit which uses a RawConnection from connection pool
    /// to comunicate with the server. 
    /// <remarks>
    /// If an connection error occure, the RawConnection is transparently replaced 
    /// by a new fresh connection.
    /// </remarks>
    /// </summary>
    public class Connection : IDisposable
    {
        private readonly IConnectionFactory _factory;
        private RawConnection _connection;
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="factory">The pool.</param>
        public Connection(IConnectionFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException ("factory");
            
            _factory = factory;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Connection"/> is reclaimed by garbage collection.
        /// </summary>
        ~Connection (){
            // make sure the connection returns to pool if the user forget it.
            Dispose (false);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is authenticated.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is authenticated; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthenticated {
            get { return _connection.IsAuthenticated; }
        }

        /// <summary>
        /// Masks as authenticated.
        /// </summary>
        public void MaskAuthenticated (){
            _connection.MarkAuthenticated ();
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString {
            get { return _factory.ConnectionString; }
        }

		/// <summary>
		/// Factory that is used to generate all returned documents
		/// </summary>
		public IDocumentFactory DocumentFactory { get; set; }

        /// <summary>
        /// Used for sending a message that gets a reply such as a query.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        /// <exception cref="IOException">A reconnect will be issued but it is up to the caller to handle the error.</exception>
        public ReplyMessage SendTwoWayMessage (IRequestMessage msg){
            if (this.State != ConnectionState.Opened) {
                throw new MongoCommException ("Operation cannot be performed on a closed connection.", this);
            }
            try {
                ReplyMessage reply = new ReplyMessage (DocumentFactory);
                lock (_connection) {
                    msg.Write (_connection.GetStream ());
                    reply.Read (_connection.GetStream ());
                }
                return reply;
            } catch (IOException) {
                ReplaceInvalidConnection ();
                throw;
            }
            
        }

        /// <summary>
        /// Used for sending a message that gets no reply such as insert or update.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        /// <exception cref="IOException">A reconnect will be issued but it is up to the caller to handle the error.</exception>        
        public void SendMessage (IRequestMessage msg){
            if (this.State != ConnectionState.Opened) {
                throw new MongoCommException ("Operation cannot be performed on a closed connection.", this);
            }
            try {
                lock (_connection) {
                    msg.Write (_connection.GetStream ());
                }
            } catch (IOException) {
                //Sending doesn't seem to always trigger the detection of a closed socket.
                ReplaceInvalidConnection ();
                throw;
            }
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>The state.</value>
        public ConnectionState State {
            get { return _connection != null ? ConnectionState.Opened : ConnectionState.Closed; }
        }

        /// <summary>
        /// Just sends a simple message string to the database. 
        /// </summary>
        /// <param name="message">
        /// A <see cref="System.String"/>
        /// </param>
        public void SendMsgMessage (String message){
            MsgMessage msg = new MsgMessage ();
            msg.Message = message;
            this.SendMessage (msg);
        }

        /// <summary>
        /// Opens this instance.
        /// </summary>
        public void Open (){
            _connection = _factory.Open();
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close (){
            if (_connection == null)
                return;

            _factory.Close(_connection);
            _connection = null;
        }

        /// <summary>
        /// Replaces the invalid connection.
        /// </summary>
        private void ReplaceInvalidConnection (){
            if (_connection == null)
                return;
            
            _connection.MarkAsInvalid ();
            _factory.Close (_connection);
            _connection = _factory.Open();
        }

        public Stream GetStream (){
            return _connection.GetStream ();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose (){
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Cleanup Managed Resources Here
                    Close();
                }

                // Cleanup Unmanaged Resources Here

                // Then mark object as disposed
                disposed = true;
            }
        }
    }
}
