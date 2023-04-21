//using Microsoft.EntityFrameworkCore;
//using P2PWallet.Models.Models.Entities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace P2PWallet.Services.Services.Seeding
//{
//    public class SecurityQuestionsService
//    {
//        DataContext _dataContext;

//        public SecurityQuestionsService(DataContext dataContext)
//        {
//            _dataContext = dataContext;
//        }
//        public static void SeedSecurityQuestions(DataContext dataContext)
//        {
//            if (dataContext.SecurityQuestions.Any())
//                return; //DB has been seeded with module data 

//            var securityquestionsList = new SecurityQuestion[]
//            {
//                new SecurityQuestion(){Question = "What city were you born in?"},
//                new SecurityQuestion(){Question = "What is your oldest sibling’s middle name?"},
//                new SecurityQuestion(){Question = "In what city or town did your parents meet?"},
//                new SecurityQuestion(){Question = "What is the make and model of your first car?"},
//                new SecurityQuestion(){Question = "What was the first concert you attended?"}
//            };
//            dataContext.ChangeTracker.Entries().Where(e => e.State == EntityState.Added);
//            dataContext.SecurityQuestions.AddRange(securityquestionsList);
//            dataContext.Database.OpenConnection();
//            try
//            {
//                dataContext.SaveChanges();
//            }
//            finally
//            {
//                dataContext.Database.CloseConnection();
//            }
//        }
//    }
//}
