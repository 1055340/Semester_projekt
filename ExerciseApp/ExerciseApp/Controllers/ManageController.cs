using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ExerciseApp.Models;
using System.Data.Entity;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace ExerciseApp.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        [HttpGet]
        public ActionResult Achievements()
        {
            AchievementsEntities context = new AchievementsEntities();
            IEnumerable<EX_AchievementTable> achievements = new List<EX_AchievementTable>();
            achievements = context.EX_AchievementTable.ToList();
            return View(achievements);
        }


        [HttpPost]
        //Json kode som tjekker om brugeren har en uvist popup efter at have tilføjet en træning
        public JsonResult GetXpPopup()
        {
            //Først henter vi det data vi skal bruge fra EX_userExercise
            using (UserInputExerciseEntities userexercisecontext = new UserInputExerciseEntities())
            {
                var userId = User.Identity.GetUserId();
                var popupexercisequeryparametres = from userexercise in userexercisecontext.EX_UserExercise
                                                   where userexercise.UserId == userId
                                                   && userexercise.UserPopupSeen == false
                                                   select new { userexercise.UserId, userexercise.ExerciseId, userexercise.ExerciseScore, userexercise.ExerciseDate };
                //Vi opdaterer den bool i databasen som siger om brugeren har modtaget popuppen, så den ikke forefindes mere end en gang
                
                //Så henter vi det data vi skal bruge fra EX_UserLevel og EX_LevelTable 
                using (UserLevelXpEntities levelcontext = new UserLevelXpEntities())
                {
                    var totalUserXp = 0;
                    var popupuserqueryparametres = from userlevel in levelcontext.EX_UserLevel
                                                   where userlevel.UserId == userId
                                                   select userlevel.UserXp;
                    foreach(var result in popupuserqueryparametres)
                    {
                        totalUserXp += result;
                    }
                    
                    //Så udregner vi nuværende xp, xp for næste level, nuværende level og næste level som skal vises i popup'en
                    var xpSum = totalUserXp;
                    var next = levelcontext.EX_LevelTable.FirstOrDefault(u => u.TotalLevelXp > xpSum);
                    int totalXpForThisLevel;
                    
                    //if statementet er til for at undgå den null-error som kommer hvis databasen returnerer 0, når vi prøver at hente brugerens level
                    if (!levelcontext.EX_LevelTable.Any(u => u.LevelId == next.LevelId - 1))
                    {
                        totalXpForThisLevel = 0;
                    }
                    else
                    {
                        totalXpForThisLevel = levelcontext.EX_LevelTable.FirstOrDefault(u => u.LevelId == next.LevelId - 1).TotalLevelXp;
                    }
                    var totalXpForThisLevelEquals = xpSum - totalXpForThisLevel;
                    var totalXpForNextLevel = next.TotalLevelXp - totalXpForThisLevel;

                    //Nu har vi informationer vi skal bruge for at vise det nye level og xp brugeren har fået, men vi skal også bruge de informationer som brugeren
                    //have før indtastningen, for at kunne animere level-ups.
                    int xptowithdraw;
                    if (!userexercisecontext.EX_UserExercise.Any(u => u.UserId == userId))
                    {
                        xptowithdraw = 0;
                    } else
                    {
                        xptowithdraw = userexercisecontext.EX_UserExercise.OrderByDescending(o => o.ExerciseDate).FirstOrDefault(u => u.UserId == userId).ExerciseScore;

                    }
                    int currentxp = xpSum - xptowithdraw;
                    int OldCurrentLevel;
                    if (!levelcontext.EX_LevelTable.Any(u => u.TotalLevelXp < currentxp))
                    {
                        OldCurrentLevel = 0;
                    }
                    else
                    {
                        OldCurrentLevel = levelcontext.EX_LevelTable.OrderByDescending(o => o.TotalLevelXp).FirstOrDefault(u => u.TotalLevelXp < currentxp).LevelId;
                    }
                    var XpForCurrentLevelEquals = currentxp - OldCurrentLevel;
                    var OldNextLevel = levelcontext.EX_LevelTable.FirstOrDefault(u => u.TotalLevelXp > currentxp);
                    var XpForNextLevelEquals = OldNextLevel.TotalLevelXp - OldCurrentLevel;


                    //Så udregner vi nuværende xp, xp for næste level, nuværende level og næste level som skal vises i popup'en
                    //var OldxpSum = OldtotalUserXp-;







                    string json = "";
                    //Så bygger vi modellen som skal sendes til javascriptet via Json
                    List<UserInputPopup> popups = new List<UserInputPopup>();
                    foreach (var result in popupexercisequeryparametres)
                    {
                        UserInputPopup popup = new UserInputPopup();
                        //Brugerens ID
                        popup.UserId = userId;

                        //ID for denne træningsøvelse
                        popup.ExerciseId = result.ExerciseId;

                        //Den mængde xp som brugeren får for denne indtastning
                        popup.ExerciseScore = result.ExerciseScore;

                        //Den relative mængde xp som brugeren har lige nu i forhold til næste level
                        popup.totalXpForThisLevelEquals = totalXpForThisLevelEquals;

                        //Den relative mængde xp som brugeren har brug for, for at nå næste level
                        popup.totalXpForNextLevel = totalXpForNextLevel;

                        //Brugerens nuværende level
                        popup.currentUserLevel = next.LevelId - 1;

                        //Brugeres næste level
                        popup.nextUserLevel = next.LevelId;

                        //Brugerens relative mængde xp, før denne indtastning (for at vise animationen i popup'en
                        popup.currentUserXp = currentxp;

                        popup.OldCurrentLevel = OldCurrentLevel;

                        popup.OldNextLevel = OldNextLevel.LevelId;

                        popup.XpForCurrentLevelEquals = XpForCurrentLevelEquals;
                        popup.XpForNextLevelEquals = XpForNextLevelEquals;
                        popups.Add(popup);
                    }

                    foreach (var unchangedpopups in userexercisecontext.EX_UserExercise.Where(x => x.UserId == userId))
                    {
                        unchangedpopups.UserPopupSeen = true;
                    }
                    userexercisecontext.SaveChanges();
                    json = JsonConvert.SerializeObject(popups);
                    return Json(json);
                }
            }
        }
        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
        var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };

            

            //Her samles de værdier som skal bruges for at vise profilsiden
            using (UserLevelXpEntities levelcontext = new UserLevelXpEntities())
            {
                //if statementet er til for at undgå den null-error som kommer hvis databasen returnerer 0, når vi prøver at hente brugerens level
                int xpSum;
                if (!levelcontext.EX_UserLevel.Any())
                {
                    xpSum = 0;
                }
                else
                {
                    var xpToSum = levelcontext.EX_UserLevel.Where(u => u.UserId == userId);
                    xpSum = xpToSum.AsQueryable().Sum(pkg => pkg.UserXp);
                }


                var next = levelcontext.EX_LevelTable.FirstOrDefault(u => u.TotalLevelXp > xpSum);

                int totalXpForThisLevel;
                //if statementet er til for at undgå den null-error som kommer hvis databasen returnerer 0, når vi prøver at hente brugerens level
                if (!levelcontext.EX_LevelTable.Any(u => u.LevelId == next.LevelId - 1))
                {
                    totalXpForThisLevel = 0;
                } else
                {
                    totalXpForThisLevel = levelcontext.EX_LevelTable.FirstOrDefault(u => u.LevelId == next.LevelId - 1).TotalLevelXp;
                }
                
                var totalXpForThisLevelEquals = xpSum - totalXpForThisLevel;
                var totalXpForNextLevel = next.TotalLevelXp - totalXpForThisLevel;

                //Informationen bindes til instancen af brugermodellen, og sendes til viewet
                model.UserTotalXp = xpSum;
                model.xpNeededForNext = next.TotalLevelXp;
                model.currentUserLevel = next.LevelId - 1;
                model.nextUserLevel = next.LevelId;
                model.totalXpForThisLevelEquals = totalXpForThisLevelEquals;
                model.totalXpForNextLevel = totalXpForNextLevel;
                IEnumerable<EX_UserLevel> totalUserXp = new List<EX_UserLevel>();
            }
            //Informationer bindes til instancen af brugermodellen, og sendes til viewet
            using (UserSettingsEntities context = new UserSettingsEntities())
            {
                EX_UserSettings user = context.EX_UserSettings.FirstOrDefault(r => r.UserId == userId);
                model.UserId = user.FacebookId;
                model.UserFirstName = user.UserFirstName;
                model.UserLastName = user.UserLastName;
                model.UserGender = user.UserGender;
                var dt = user.UserBirthday;
                model.UserBirthday = String.Format("{0:dd/MM/yyyy}", dt);
            };
            
            return View(model);
        }

        //private static void NewMethod(IndexViewModel model, IEnumerable<EX_UserLevel> totalUserXp)
        //{
        //    model.UserTotalXp = totalUserXp;
        //}

        [HttpGet]
        public ActionResult Categories()
        {   
            //Henter listen med alle exercises fra databasen, og tilføjer dem til 'exercises' list-elementet fra modellen
            CategoryEntities context = new CategoryEntities();
            IEnumerable<EX_ExerciseTable> exercises = new List<EX_ExerciseTable>();
            exercises = context.EX_ExerciseTable.ToList();
            return View(exercises);
        }
        [HttpPost]
        public ActionResult Create(EX_UserExercise newExercise)
        {
            //Her oprettes der en ny entry i databasen med brugerens indtastede information
                UserExerciseViewModeltest exercise = new UserExerciseViewModeltest();
                var userid = User.Identity.GetUserId();
            //exercisevalue1, 2 og 3 er kg, reps og sets. Disse værdier er sat til at være 1, i tilfælde af at der indtastes en træning med løb eller lign
            //så hvis der er løbet 5km, er regnestykket 5*1*1, hvilket stadig er 5.
                var exerciseValue = (newExercise.ExerciseValue1 * newExercise.ExerciseValue2 * newExercise.ExerciseValue3);
                var ExerciseScorestep1 = exerciseValue * newExercise.ExerciseMultiplier;

            //Outputtet divideres med 100, for at komme udenom en fejl hvor et kommatal ikke kunne sendes til controlleren, og derfor
            //blev ganget med 100 for at give et helt tal
                var ExerciseScoreResult = ExerciseScorestep1 / 100;
            using (UserInputExerciseEntities context = new UserInputExerciseEntities())
            {
                EX_UserExercise userExercise = new EX_UserExercise
                {
                    UserId = User.Identity.GetUserId(),
                    ExerciseId = newExercise.ExerciseId,
                    ExerciseValue = exerciseValue,
                    ExerciseScore = ExerciseScoreResult,
                    ExerciseDate = DateTime.Now,
                    UserPopupSeen = false,
                };
                context.EX_UserExercise.Add(userExercise);
                context.SaveChanges();
            }

            //Her tilføjes et entry til userlevel, for at holde styr på brugerens xp
            using (UserLevelXpEntities levelContext = new UserLevelXpEntities())
            {
                EX_UserLevel userXp = new EX_UserLevel
                {
                    UserId = User.Identity.GetUserId(),
                    UserXp = ExerciseScoreResult,
                };
                levelContext.EX_UserLevel.Add(userXp);
                levelContext.SaveChanges();
            }
            return RedirectToAction("Index", "Manage");
        }



        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}