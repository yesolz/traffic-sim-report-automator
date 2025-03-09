using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Simulator
{
	public class DelegateCommand : ICommand
	{
		private readonly Func<bool> _canExecute;
		private readonly Action _execute;

		/// <summary>
		/// Initializes a new instance of the DelegateCommand class.
		/// </summary>
		/// <param name="execute">indicate an execute function</param>
		public DelegateCommand(Action execute) : this(execute, null!)
		{
			
		}

		/// <summary>
		/// Initializes a new instance of the DelegateCommand class.
		/// </summary>
		/// <param name="execute">execute function </param>
		/// <param name="canExecute">can execute function</param>
		public DelegateCommand(Action execute, Func<bool> canExecute)
		{
			this._execute = execute;
			this._canExecute = canExecute;
		}
		/// <summary>
		/// can executes event handler
		/// </summary>
		public event EventHandler? CanExecuteChanged;

		/// <summary>
		/// implement of icommand can execute method
		/// </summary>
		/// <param name="o">parameter by default of icomand interface</param>
		/// <returns>can execute or not</returns>
		public bool CanExecute(object? o)
		{
			if (this._canExecute == null)
			{
				return true;
			}
			return this._canExecute();
		}

		/// <summary>
		/// implement of icommand interface execute method
		/// </summary>
		/// <param name="o">parameter by default of icomand interface</param>
		public void Execute(object? o)
		{
			this._execute();
		}

		/// <summary>
		/// raise ca excute changed when property changed
		/// </summary>
		public void RaiseCanExecuteChanged([CallerMemberName] string? propertyName = null)
		{
			if (this.CanExecuteChanged != null)
			{
				this.CanExecuteChanged(this, EventArgs.Empty);
			}
		}
	}
}
