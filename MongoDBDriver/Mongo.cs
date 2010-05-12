using System;
using MongoDB.Driver.Connections;

namespace MongoDB.Driver
{
    /// <summary>
    /// Description of Mongo.
    /// </summary>
	public class Mongo : IDisposable, IMongo
    {
        private Connection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mongo"/> class.
        /// </summary>
        public Mongo () : this(string.Empty){
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mongo"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
		public Mongo(string connectionString) : this(connectionString, null){
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Mongo"/> class.
		/// </summary>
		/// <param name="documentFactory">The document factory for <see cref="Document" /></param>
		public Mongo(IDocumentFactory documentFactory) : this(string.Empty, documentFactory){
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Mongo"/> class.
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="documentFactory">The document factory for <see cref="Document" /></param>
		public Mongo(string connectionString, IDocumentFactory documentFactory)
		{
            if (connectionString == null)
                throw new ArgumentNullException ("connectionString");
            
            connection = ConnectionFactory.GetConnection (connectionString);
			connection.DocumentFactory = documentFactory ?? new DefaultDocumentFactory();
		}

		/// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString {
            get { return connection.ConnectionString; }
        }

        /// <summary>
        /// Gets the named database.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Database GetDatabase (String name){
            return new Database (connection, name);
        }

        /// <summary>
        /// Gets the <see cref="MongoDB.Driver.Database"/> with the specified name.
        /// </summary>
        /// <value></value>
        public Database this[String name] {
            get { return this.GetDatabase (name); }
        }

        /// <summary>
        /// Connects this instance.
        /// </summary>
        /// <returns></returns>
        public Boolean Connect (){
            connection.Open ();
            return connection.State == ConnectionState.Opened;
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        /// <returns></returns>
        public Boolean Disconnect (){
            connection.Close ();
            return connection.State == ConnectionState.Closed;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose (){
            connection.Dispose ();
        }
    }
}
