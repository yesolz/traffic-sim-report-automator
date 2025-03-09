using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator
{
	internal class AsyncCommand : Command.AsyncCommand.AsyncCommandBase
	{
		private readonly Func<Task> _command;
		public AsyncCommand(Func<Task> command)
		{
			_command = command;
		}

		public override bool CanExecute(object? parameter)
		{
			return true;
		}

		public override Task ExecuteAsync(object parameter)
		{
			return _command();
		}
	}
}
