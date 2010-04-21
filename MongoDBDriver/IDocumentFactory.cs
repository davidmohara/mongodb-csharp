using System;

namespace MongoDB.Driver
{
    public interface IDocumentFactory
	{
		Document CreateDocument();
	}
}
