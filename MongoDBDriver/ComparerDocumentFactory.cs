using System;
using System.Collections.Generic;

namespace MongoDB.Driver
{
	public class ComparerDocumentFactory : IDocumentFactory
	{
		private IComparer<string> comparer;

		public ComparerDocumentFactory(IComparer<string> comparer)
		{
			this.comparer = comparer;
		}

		public Document CreateDocument()
		{
			return new Document(comparer);
		}
	}
}
