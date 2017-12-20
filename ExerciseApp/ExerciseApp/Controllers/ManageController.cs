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
using Facebook;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;

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
        public ActionResult Home()
        {
            var userId = User.Identity.GetUserId();
            var model = new HomeViewModel{};

            //Her samles de værdier som skal bruges for at vise profilsiden
            using (UserLevelXpEntities levelcontext = new UserLevelXpEntities())
            {
                //if statementet er til for at undgå den null-error som kommer hvis databasen returnerer 0, når vi prøver at hente brugerens level
                int xpSum;
                if (!levelcontext.EX_UserLevel.Where(u => u.UserId == userId).Any())
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
                }
                else
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
                model.currentDay = DateTime.Now.DayOfWeek.ToString();
                model.currentDate = DateTime.Now.Date.ToShortDateString();
                using (AchievementsEntities achcontext = new AchievementsEntities())
                {
                    var totalachievements = achcontext.EX_AchievementTable.Count();

                    model.totalAchievements = totalachievements;
                }
                using (UserAchievementEntities userachcontext = new UserAchievementEntities())
                {
                    var unlockedachievements = userachcontext.EX_UserAchievement.Where(u => u.UserId == userId).Count();

                    model.unlockedAchievements = unlockedachievements;
                }
                    
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult ChallengeFriendList()
        {
            using (UserSettingsEntities context = new UserSettingsEntities())
            {
                //Facebook SDK
                var userId = User.Identity.GetUserId();
                var access_token = context.EX_UserSettings.FirstOrDefault(u => u.UserId == userId).FacebookToken;
                var fb = new FacebookClient(access_token);
                dynamic myInfo = fb.Get("/me?fields=friends{id,name,picture{url}}");
                var data = myInfo["friends"].ToString();
                RootObject response = JsonConvert.DeserializeObject<RootObject>(data);
                return View(response);
            }
        }

        [HttpGet]
        public ActionResult Achievements()
        {
            //Listen med alle achievements hentes i databasen
            AchievementsEntities context = new AchievementsEntities();
            IEnumerable<EX_AchievementTable> achievements = new List<EX_AchievementTable>();
            achievements = context.EX_AchievementTable.ToList();
            return View(achievements);
        }
        [HttpPost]
        public JsonResult GetUserAchievements()
        {
            //de achievements som brugeren har opnået, hentes i databasen
            var userId = User.Identity.GetUserId();
            using (UserAchievementEntities userachievementcontext = new UserAchievementEntities())
            {
                using (AchievementsEntities achievements = new AchievementsEntities())
                {
                    string json = "";
                    IEnumerable<EX_UserAchievement> userachievements = new List<EX_UserAchievement>();
                    IEnumerable<EX_AchievementTable> userachievementinfo = new List<EX_AchievementTable>();
                    userachievements = userachievementcontext.EX_UserAchievement.Where(u => u.UserId == userId).Where(u => u.UserSeen == false).ToList();
                    List<UserAchievementPopup> popups = new List<UserAchievementPopup>();
                    foreach (var item in userachievements)
                    {
                        userachievementinfo = achievements.EX_AchievementTable.Where(u => u.AchievementId == item.AchievementId);
                        foreach (var achs in userachievementinfo)
                        {
                            UserAchievementPopup unlockedachievement = new UserAchievementPopup();
                            unlockedachievement.AchievementId = item.AchievementId;
                            unlockedachievement.AchievementName = achs.AchievementName;
                            unlockedachievement.AchievementDescription = achs.AchievementDescription;
                            unlockedachievement.AchievementScore = achs.AchievementScore;
                            popups.Add(unlockedachievement);
                        }
                    }
                    //Værdien i databasen for om brugeren har set popup'en ændres, så den ikke vises mere end en gang
                    foreach (var unseenpopups in userachievementcontext.EX_UserAchievement.Where(x => x.UserId == userId))
                    {
                        unseenpopups.UserSeen = true;
                    }
                    userachievementcontext.SaveChanges();

                    json = JsonConvert.SerializeObject(popups);
                    return Json(json);
                }
            }
        }

        [HttpPost]
        public JsonResult UserAchievementsUpdated()
        {
            using (UserAchievementEntities userachievementcontext = new UserAchievementEntities())
            {
                var userId = User.Identity.GetUserId();
                string json = "";
                IEnumerable<EX_UserAchievement> UserAchievementList = new List<EX_UserAchievement>();
                UserAchievementList = userachievementcontext.EX_UserAchievement.Where(u => u.UserId == userId).ToList();
                json = JsonConvert.SerializeObject(UserAchievementList);
                return Json(json);
            }
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
                    foreach (var result in popupuserqueryparametres)
                    {
                        totalUserXp += result;
                    }

                    //Så udregner vi nuværende xp, xp for næste level, nuværende level og næste level som skal vises i popup'en
                    var xpSum = totalUserXp;
                    var next = levelcontext.EX_LevelTable.FirstOrDefault(u => u.TotalLevelXp > xpSum);

                    //Tjekker om brugerens næste level udløser et achievement
                    using (UserAchievementEntities userachievements = new UserAchievementEntities())
                    {
                        var currentlevel = next.LevelId;
                        if (currentlevel > 1 && !userachievements.EX_UserAchievement.Any(u => u.AchievementId == 1 && u.UserId == userId))
                        {
                            EX_UserAchievement uachievement1 = new EX_UserAchievement();
                            uachievement1.AchievementId = 1;
                            uachievement1.UserId = userId;
                            uachievement1.UserSeen = false;
                            userachievements.EX_UserAchievement.Add(uachievement1);
                        }

                        if (currentlevel > 5 && !userachievements.EX_UserAchievement.Any(u => u.AchievementId == 2 && u.UserId == userId))
                        {
                            EX_UserAchievement uachievement2 = new EX_UserAchievement();
                            uachievement2.AchievementId = 2;
                            uachievement2.UserId = userId;
                            uachievement2.UserSeen = false;
                            userachievements.EX_UserAchievement.Add(uachievement2);
                        }

                        if (currentlevel > 10 && !userachievements.EX_UserAchievement.Any(u => u.AchievementId == 3 && u.UserId == userId))
                        {
                            EX_UserAchievement uachievement3 = new EX_UserAchievement();
                            uachievement3.AchievementId = 3;
                            uachievement3.UserId = userId;
                            uachievement3.UserSeen = false;
                            userachievements.EX_UserAchievement.Add(uachievement3);
                        }


                        userachievements.SaveChanges();
                    }

                    //Achievements end
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
                    }
                    else
                    {
                        xptowithdraw = userexercisecontext.EX_UserExercise.OrderByDescending(o => o.ExerciseDate).FirstOrDefault(u => u.UserId == userId).ExerciseScore;

                    }
                    int currentxp = xpSum - xptowithdraw;
                    int OldCurrentLevel;
                    var OldCurrentLevelXp = 0;
                    if (!levelcontext.EX_LevelTable.Any(u => u.TotalLevelXp < currentxp))
                    {
                        OldCurrentLevel = 0;
                        OldCurrentLevel = 0;
                    }
                    else
                    {
                        OldCurrentLevel = levelcontext.EX_LevelTable.OrderByDescending(o => o.TotalLevelXp).FirstOrDefault(u => u.TotalLevelXp < currentxp).LevelId;
                        OldCurrentLevelXp = levelcontext.EX_LevelTable.OrderByDescending(o => o.TotalLevelXp).FirstOrDefault(u => u.TotalLevelXp < currentxp).TotalLevelXp;
                    }
                    //Så udregner vi nuværende xp, xp for næste level, nuværende level og næste level som skal vises i popup'en
                    var XpForCurrentLevelEquals = currentxp - OldCurrentLevelXp;
                    var OldNextLevel = levelcontext.EX_LevelTable.FirstOrDefault(u => u.TotalLevelXp > currentxp);
                    var XpForNextLevelEquals = OldNextLevel.TotalLevelXp - OldCurrentLevelXp;



                    //Tjekker om brugeren har opnået achievement 6, 7, 8, 9, 10 eller 11
                    using (UserAchievementEntities userachievements = new UserAchievementEntities())
                    {
                        //Tjekker for achievement 6
                        var alreadyHasAchievement6 = userachievements.EX_UserAchievement.Any(u => u.AchievementId == 6 && u.UserId == userId);
                        if (alreadyHasAchievement6 == false)
                        {
                            var Achievement6 = userexercisecontext.EX_UserExercise.Where(u => u.ExerciseId == 8);
                            var totalrunningkm = 0;
                            foreach (var runningkm in Achievement6)
                            {
                                totalrunningkm += runningkm.ExerciseValue;
                            }
                            if (totalrunningkm >= 42)
                            {
                                EX_UserAchievement achievement6 = new EX_UserAchievement();
                                achievement6.AchievementId = 6;
                                achievement6.UserId = userId;
                                achievement6.UserSeen = false;
                                userachievements.EX_UserAchievement.Add(achievement6);
                            }
                        }
                        //Tjekker for achievement 7
                        var alreadyHasAchievement7 = userachievements.EX_UserAchievement.Any(u => u.AchievementId == 7 && u.UserId == userId);
                        if (alreadyHasAchievement7 == false)
                        {
                            var Achievement7 = userexercisecontext.EX_UserExercise.Where(u => u.ExerciseId == 6);
                            var totalrowingkm = 0;
                            foreach (var rowingkm in Achievement7)
                            {
                                totalrowingkm += rowingkm.ExerciseValue;
                            }
                            if (totalrowingkm >= 250)
                            {
                                EX_UserAchievement achievement7 = new EX_UserAchievement();
                                achievement7.AchievementId = 7;
                                achievement7.UserId = userId;
                                achievement7.UserSeen = false;
                                userachievements.EX_UserAchievement.Add(achievement7);
                            }
                        }
                        //Tjekker for achievement 8
                        var alreadyHasAchievement8 = userachievements.EX_UserAchievement.Any(u => u.AchievementId == 8 && u.UserId == userId);
                        if (alreadyHasAchievement8 == false)
                        {
                            var Achievement8 = userexercisecontext.EX_UserExercise.Where(u => u.ExerciseId == 3);
                            var totalbenchingkg = 0;
                            foreach (var benchingkg in Achievement8)
                            {
                                totalbenchingkg += benchingkg.ExerciseValue;
                            }
                            if (totalbenchingkg >= 10000)
                            {
                                EX_UserAchievement achievement8 = new EX_UserAchievement();
                                achievement8.AchievementId = 8;
                                achievement8.UserId = userId;
                                achievement8.UserSeen = false;
                                userachievements.EX_UserAchievement.Add(achievement8);
                            }
                        }
                        //Tjekker for achievement 9
                        var alreadyHasAchievement9 = userachievements.EX_UserAchievement.Any(u => u.AchievementId == 9 && u.UserId == userId);
                        if (alreadyHasAchievement9 == false)
                        {
                            var Achievement9 = userexercisecontext.EX_UserExercise.Where(u => u.ExerciseId == 9);
                            var totalbikekm = 0;
                            foreach (var bikekm in Achievement9)
                            {
                                totalbikekm += bikekm.ExerciseValue;
                            }
                            if (totalbikekm >= 250)
                            {
                                EX_UserAchievement achievement9 = new EX_UserAchievement();
                                achievement9.AchievementId = 9;
                                achievement9.UserId = userId;
                                achievement9.UserSeen = false;
                                userachievements.EX_UserAchievement.Add(achievement9);
                            }
                        }
                        //Tjekker for achievement 11 (triathlon) (achievement 10 er: vind en challenge og tjekkes et andet sted)
                        var alreadyHasAchievement11 = userachievements.EX_UserAchievement.Any(u => u.AchievementId == 11 && u.UserId == userId);
                        if (alreadyHasAchievement11 == false)
                        {
                            var Achievement11swim = userexercisecontext.EX_UserExercise.Where(u => u.ExerciseId == 10);
                            var totalswimkm = 0;
                            foreach (var swimkm in Achievement11swim)
                            {
                                totalswimkm += swimkm.ExerciseValue;
                            }
                            var Achievement11bike = userexercisecontext.EX_UserExercise.Where(u => u.ExerciseId == 9);
                            var totalbikekm = 0;
                            foreach (var bikekm in Achievement11bike)
                            {
                                totalbikekm += bikekm.ExerciseValue;
                            }
                            var Achievement11run = userexercisecontext.EX_UserExercise.Where(u => u.ExerciseId == 8);
                            var totalrunkm = 0;
                            foreach (var runkm in Achievement11bike)
                            {
                                totalrunkm += runkm.ExerciseValue;
                            }
                            if (totalswimkm >= 1.5 && totalbikekm >= 40 && totalrunkm >= 10)
                            {
                                EX_UserAchievement achievement11 = new EX_UserAchievement();
                                achievement11.AchievementId = 11;
                                achievement11.UserId = userId;
                                achievement11.UserSeen = false;
                                userachievements.EX_UserAchievement.Add(achievement11);
                            }
                        }
                        userachievements.SaveChanges();
                    }
                    //Achievements end


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
                    //Værdien i databasen for om brugeren har set popup'en ændres, så den ikke vises mere end en gang
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
        [HttpPost]
        public JsonResult ChartData()
        {
            using (UserInputExerciseEntities usercontext = new UserInputExerciseEntities())
            {
                using (CategoryEntities exercisecontext = new CategoryEntities())
                {
                    string json = "test";
                var userId = User.Identity.GetUserId();

                var query = from s in usercontext.EX_UserExercise
                            where s.UserId == userId
                            group s by s.ExerciseId into g  // group by member Id
                            let loginsCount = g.Count()  // get count of entries for each member
                            orderby loginsCount descending // order by entries count
                            select new
                            { // create new anonymous object with all data you need
                                Exercise = g.Key,
                                InputCount = loginsCount,
                            };
                var top5 = query.Take(5);
                List<ChartDataViewModel> topExercises = new List<ChartDataViewModel>();
                foreach (var item in top5)
                {
                    
                        ChartDataViewModel challengeListInfo = new ChartDataViewModel();
                        
                        var exerciseName = exercisecontext.EX_ExerciseTable.FirstOrDefault(u => u.ExerciseId == item.Exercise).ExerciseName;
                        challengeListInfo.ExerciseName = exerciseName;
                        challengeListInfo.ExerciseCount = item.InputCount;
                        topExercises.Add(challengeListInfo);
                   
                }
                    int totalKGLifted = 0;
                    var cardioInputs = usercontext.EX_UserExercise.Where(u => u.UserId == userId && u.ExerciseId == 6 || u.ExerciseId == 7 || u.ExerciseId == 8 || u.ExerciseId == 9 || u.ExerciseId == 10).Count();
                    var strengthInputs = usercontext.EX_UserExercise.Where(u => u.UserId == userId && u.ExerciseId == 1 || u.ExerciseId == 2 || u.ExerciseId == 3 || u.ExerciseId == 4 || u.ExerciseId == 5).Count();
                    var totalKGLiftedquery = usercontext.EX_UserExercise.Where(u => u.UserId == userId && u.ExerciseId == 1 || u.ExerciseId == 2 || u.ExerciseId == 3 || u.ExerciseId == 4 || u.ExerciseId == 5);
                    var totalDistancequery = usercontext.EX_UserExercise.Where(u => u.UserId == userId && u.ExerciseId == 1 || u.ExerciseId == 7 || u.ExerciseId == 8 || u.ExerciseId == 9 || u.ExerciseId == 10);
                    List<ChartDataViewModel> exerciseInputList = new List<ChartDataViewModel>();
                    foreach (var item in totalKGLiftedquery)
                    {
                        ChartDataViewModel totalInputs = new ChartDataViewModel();
                        totalInputs.strengthInput = item.ExerciseValue;
                        exerciseInputList.Add(totalInputs);
                    }
                var result = new { TopExercises = topExercises, TotalKg = exerciseInputList, CardioInputs = cardioInputs, StrengthInputs = strengthInputs };
                return Json(result, JsonRequestBehavior.AllowGet);
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
                if (!levelcontext.EX_UserLevel.Where(u => u.UserId == userId).Any())
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
                }
                else
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

        

        [HttpGet]
        public ActionResult Categories()
        {
            //Henter listen med alle exercises fra databasen, og tilføjer dem til 'exercises' list-elementet fra modellen
            CategoryEntities context = new CategoryEntities();
            IEnumerable<EX_ExerciseTable> exercises = new List<EX_ExerciseTable>();
            exercises = context.EX_ExerciseTable.ToList();
            return View(exercises);
        }
        [HttpGet]
        public ActionResult ChallengeCategories()
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
            UserExerciseViewModel exercise = new UserExerciseViewModel();
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


        [HttpGet]
        public ActionResult GetChallenges()
        {
            using (mmda0915_1055358Entities4 context = new mmda0915_1055358Entities4())
            {
                using (UserSettingsEntities usercontext = new UserSettingsEntities())
                {
                    //Henter listen med alle challenges fra databasen, og tilføjer dem til 'challenges' list-elementet fra modellen
                    var userid = User.Identity.GetUserId();



                    IEnumerable<EX_ChallengeTable> challenges = new List<EX_ChallengeTable>();
                    var results = context.EX_ChallengeTable.Where(u => u.ChallengedId == userid || u.ChallengerId == userid);
                    //challenges = context.EX_ChallengeTable.Where(u => u.ChallengedId == userid || u.ChallengerId == userid).ToList();
                    List<EX_ChallengeTable> challengeView = new List<EX_ChallengeTable>();
                    foreach (var item in results)
                    {
                        EX_ChallengeTable challengeListInfo = new EX_ChallengeTable();
                        var challengerFirstname = "";
                        var challengerLastname = "";
                        var challengerId = item.ChallengerId;
                        var challengedId = item.ChallengedId;
                        if (challengerId == usercontext.EX_UserSettings.FirstOrDefault(u => u.UserId == userid).UserId)
                        {
                            challengerFirstname = usercontext.EX_UserSettings.FirstOrDefault(u => u.UserId == item.ChallengedId).UserFirstName;
                            challengerLastname = usercontext.EX_UserSettings.FirstOrDefault(u => u.UserId == item.ChallengedId).UserLastName;
                        }
                        else
                        {
                            challengerFirstname = usercontext.EX_UserSettings.FirstOrDefault(u => u.UserId == item.ChallengerId).UserFirstName;
                            challengerLastname = usercontext.EX_UserSettings.FirstOrDefault(u => u.UserId == item.ChallengerId).UserLastName;
                        }
                        var challengerFullname = challengerFirstname + " " + challengerLastname;
                        challengeListInfo.ChallengerName = challengerFullname;
                        using (CategoryEntities exercisecontext = new CategoryEntities())
                        {
                            var exerciseName = exercisecontext.EX_ExerciseTable.FirstOrDefault(u => u.ExerciseId == item.ExerciseId).ExerciseName;
                            challengeListInfo.exerciseName = exerciseName;
                        }
                        if (item.ChallengedId == userid && item.ChallengedAccepted == false)
                        {
                            challengeListInfo.Areyouchallenged = 1;
                        }
                        else
                        {
                            challengeListInfo.Areyouchallenged = 0;
                        }
                        var hest = item.ChallengeWinner;
                        var tes = usercontext.EX_UserSettings.FirstOrDefault(u => u.UserId == userid).FacebookId;
                        string vest = usercontext.EX_UserSettings.FirstOrDefault(u => u.UserId == userid).FacebookId;
                        if (item.ChallengeWinner == usercontext.EX_UserSettings.FirstOrDefault(u => u.UserId == userid).FacebookId)
                        {
                            challengeListInfo.ThisUserWon = 1;
                        } else
                        {
                            challengeListInfo.ThisUserWon = 0;
                        }
                        challengeListInfo.ChallengeId = item.ChallengeId;
                        challengeListInfo.ChallengeGoal = item.ChallengeGoal;
                        challengeListInfo.ChallengeStart = item.ChallengeStart;
                        challengeListInfo.ChallengeEnd = item.ChallengeEnd;
                        challengeListInfo.ChallengeTitle = item.ChallengeTitle;
                        challengeView.Add(challengeListInfo);
                    }
                    return View(challengeView);

                }
            }

        }
        [HttpPost]
        public JsonResult GetChallengeDetails(int id)
        {
            string json = "";

            using (mmda0915_1055358Entities4 context = new mmda0915_1055358Entities4())
            {
                using (UserSettingsEntities usercontext = new UserSettingsEntities())
                {
                    using (CategoryEntities exercisecontext = new CategoryEntities())
                    {
                        using (UserInputExerciseEntities inputcontext = new UserInputExerciseEntities())
                        {
                            var result = context.EX_ChallengeTable.Where(u => u.ChallengeId == id);

                            List<GetChallengeDetails> challengeDetails = new List<GetChallengeDetails>();
                            foreach (var item in result)
                            {
                                GetChallengeDetails challengedetails = new GetChallengeDetails();
                                challengedetails.ThisUser = User.Identity.GetUserId();
                                challengedetails.ChallengeId = item.ChallengeId;
                                challengedetails.ChallengerId = usercontext.EX_UserSettings.FirstOrDefault(u => u.UserId == item.ChallengerId).FacebookId;
                                challengedetails.ChallengedId = usercontext.EX_UserSettings.FirstOrDefault(u => u.UserId == item.ChallengedId).FacebookId;
                                //Først skal vi have fat i challengerens navn
                                challengedetails.ChallengerName = usercontext.EX_UserSettings.FirstOrDefault(u => u.UserId == item.ChallengerId).UserFirstName;
                                challengedetails.ChallengedName = usercontext.EX_UserSettings.FirstOrDefault(u => u.UserId == item.ChallengedId).UserFirstName;
                                challengedetails.ExerciseName = exercisecontext.EX_ExerciseTable.FirstOrDefault(u => u.ExerciseId == item.ExerciseId).ExerciseName;
                                var challengerinputs = inputcontext.EX_UserExercise.Where(u => u.UserId == item.ChallengerId && u.ExerciseId == item.ExerciseId && u.ExerciseDate >= item.ChallengeStart && u.ExerciseDate <= item.ChallengeEnd).ToList();
                                var challengerinputtotal = 0;
                                foreach (var challengerinput in challengerinputs)
                                {
                                    challengerinputtotal += challengerinput.ExerciseValue;
                                };
                                challengedetails.ChallengerValue = challengerinputtotal;
                                var challengedinputs = inputcontext.EX_UserExercise.Where(u => u.UserId == item.ChallengedId && u.ExerciseId == item.ExerciseId && u.ExerciseDate >= item.ChallengeStart && u.ExerciseDate <= item.ChallengeEnd).ToList();
                                var challengedinputtotal = 0;
                                foreach (var challengedinput in challengedinputs)
                                {
                                    challengedinputtotal += challengedinput.ExerciseValue;
                                };
                                challengedetails.ChallengedValue = challengedinputtotal;
                                challengedetails.ChallengeTitle = item.ChallengeTitle;
                                challengedetails.ChallengeGoal = item.ChallengeGoal;
                                challengedetails.ChallengeScore = item.ChallengeScore;
                                challengedetails.ChallengeStart = item.ChallengeStart;
                                challengedetails.ChallengeEnded = item.ChallengeEnded;
                                challengedetails.ChallengeEnd = item.ChallengeEnd;

                                challengeDetails.Add(challengedetails);

                            }
                            return Json(challengeDetails);
                        }
                    }
                }
            }
        }
        [HttpPost]
        public JsonResult AcceptChallenge(int id)
        {
            string json = "";

            using (mmda0915_1055358Entities4 context = new mmda0915_1055358Entities4())
            {
                var result = context.EX_ChallengeTable.FirstOrDefault(u => u.ChallengeId == id);
                result.ChallengedAccepted = true;
                result.ChallengeStart = DateTime.Now;
                context.Entry(result).State = EntityState.Modified;
                context.SaveChanges();
            }

            return Json(json);
        }
        [HttpPost]
        public JsonResult DeleteChallenge(int id)
        {
            string json = "";

            using (mmda0915_1055358Entities4 context = new mmda0915_1055358Entities4())
            {
                var result = context.EX_ChallengeTable.FirstOrDefault(u => u.ChallengeId == id);
                context.EX_ChallengeTable.Remove(result);
                context.SaveChanges();
            }
            return Json(json);
        }


        [HttpPost]
        public ActionResult CreateChallenge(EX_ChallengeTable newChallenge)
        {
            using (mmda0915_1055358Entities4 context = new mmda0915_1055358Entities4())
            {
                using (UserSettingsEntities usercontext = new UserSettingsEntities())
                {
                    var challengedId = usercontext.EX_UserSettings.FirstOrDefault(u => u.FacebookId == newChallenge.ChallengedId).UserId;





                    EX_ChallengeTable challengetable = new EX_ChallengeTable()
                    {
                        ChallengerId = User.Identity.GetUserId(),
                        ChallengedId = challengedId,
                        ChallengedAccepted = false,
                        ChallengeEnded = false,
                        ExerciseId = newChallenge.ExerciseId,
                        ChallengeScore = 150,
                        ChallengeTitle = newChallenge.ChallengeTitle.ToUpper(),
                        ChallengeGoal = newChallenge.ChallengeGoal,
                        ChallengeStart = DateTime.Now,
                        ChallengeEnd = newChallenge.ChallengeEnd,
                    };

                    context.EX_ChallengeTable.Add(challengetable);
                    context.SaveChanges();


                    return RedirectToAction("Index", "Manage");
                }
            }
        }

        [HttpPost]
        public JsonResult FinishChallenge(string winner, int id)
        {
            string json = "";
            using (UserSettingsEntities usercontext = new UserSettingsEntities())
            {
                var winnerName = usercontext.EX_UserSettings.FirstOrDefault(u => u.FacebookId == winner).UserFirstName;
                json = winnerName;
            }
            
            var userId = User.Identity.GetUserId();

            using (mmda0915_1055358Entities4 context = new mmda0915_1055358Entities4())
            {
                var result = context.EX_ChallengeTable.FirstOrDefault(u => u.ChallengeId == id);
                result.ChallengeWinner = winner;
                result.ChallengeEnded = true;
                context.Entry(result).State = EntityState.Modified;
                context.SaveChanges();
            }
            using (UserAchievementEntities achievementcontext = new UserAchievementEntities())
            {
                if (!achievementcontext.EX_UserAchievement.Any(u => u.AchievementId == 10 && u.UserId == userId))
                {
                    EX_UserAchievement uachievement10 = new EX_UserAchievement();
                    uachievement10.AchievementId = 10;
                    uachievement10.UserId = userId;
                    uachievement10.UserSeen = false;
                    achievementcontext.EX_UserAchievement.Add(uachievement10);
                }
                achievementcontext.SaveChanges();
            }
            


            return Json(json);
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