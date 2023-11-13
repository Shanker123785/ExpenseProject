using Expenses.Core.DTO;
using DB = Expenses.DB;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ExpensesCore
{
    public class ExpensesServices : IExpensesServices
    {
        private DB.AppDbContext _context;
        private readonly DB.User _user;
        public ExpensesServices(DB.AppDbContext context,IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _user = _context.Users
                .First(u => u.Username == httpContextAccessor.HttpContext.User.Identity.Name);
        }

        public Expense CreateExpense(DB.Expense expense)
        {
            expense.User = _user;       //we then create a dto so that the user data isnt returned when returning the expense object here. Make note this the use of DTO's
            _context.Add(expense);
            _context.SaveChanges();
            return (Expense)expense; //Explicit converter added to dto expense class for this to work. Parsing is done here.
        }

        public void DeleteExpense(Expense expense)
        {
            var dbExpense = _context.Expenses.First(e => e.User.Id == _user.Id && e.Id == expense.Id);
            //_context.Expenses.Remove(expense);
            _context.Expenses.Remove(dbExpense);
            _context.SaveChanges();
        }

        public Expense EditExpense(Expense expense)
        {
            var dbExpense = _context.Expenses.First(e => e.User.Id == _user.Id && e.Id == expense.Id);
            dbExpense.Description = expense.Description;
            dbExpense.Amount = expense.Amount;
            _context.SaveChanges();
            return expense;
        }

        //public Expense GetExpense(int id)
        //{
        //    return _context.Expenses.First(e => e.Id == id);
        //}
        public Expense GetExpense(int id)=>
            _context.Expenses
            .Where(e=>e.User.Id == _user.Id && e.Id==id)
            .Select(e=>(Expense)e)
            .First();
        public List<Expense> GetExpenses() =>
            _context.Expenses
            .Where(e => e.User.Id == _user.Id)
            .Select(e => (Expense)e)
            .ToList();
        
            //return new List<Expense> { new Expense() };
            //if(_context.Expenses.Any())
            //    return _context.Expenses.ToList();
            //else
            //    return new List<Expense> {};
          
        
    }
}