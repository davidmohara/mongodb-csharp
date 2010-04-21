
namespace MongoDB.Driver
{
	public class DefaultDocumentFactory : IDocumentFactory
	{
		public Document CreateDocument()
		{
			return new Document();
		}
	}
}
