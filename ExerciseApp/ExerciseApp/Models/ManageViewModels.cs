using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Data.Entity;

namespace ExerciseApp.Models
{
    public class IndexViewModel
    {
        public string UserId { get; set; }
        public int UserTotalXp { get; set; }
        public int currentUserLevel { get; set; }
        public int xpNeededForNext { get; set; }
        public int nextUserLevel { get; set; }
        public int totalXpForThisLevelEquals { get; set; }
        public int totalXpForNextLevel { get; set; }
        public string UserEmail { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserGender { get; set; }
        public string FacebookToken { get; set; }
        public string UserBirthday { get; set; }

        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
    }
    public class UserAchievements : DbContext
    {
        public string UserId { get; set; }
        public int AchievementId { get; set; }
        public int id { get; set; }
        public virtual IEnumerable<UserAchievements> Achievements { get; set; }
    }
    public class Achievements : DbContext
    {
        public int AchievementId { get; set; }
        public string AchievementName { get; set; }
        public string AchievementDescription { get; set; }
        public int AchievementScore { get; set; }
    }
    public class CategoriesViewModel : DbContext
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; }
        public bool ExerciseType { get; set; }
        public double ExerciseMultiplier { get; set; }
        public virtual IEnumerable<EX_ExerciseTable> Exercises { get; set; }
    }
    public class UserExerciseViewModeltest : DbContext
    {
        public string UserId { get; set; }
        public int ExerciseId { get; set; }
        public int ExerciseValue { get; set; }
        public int ExerciseValue1 { get; set; }
        public int ExerciseValue2 { get; set; }
        public int ExerciseValue3 { get; set; }
        public int ExerciseMultiplier { get; set; }
        public int ExerciseScore { get; set; }
        public System.DateTime ExerciseDate { get; set; }
        public bool UserPopupSeen { get; set; }
    }
    public class UserInputPopup
    {
        public virtual IEnumerable<UserInputPopup> PopUps { get; set; }
        public string UserId { get; set; }
        public int ExerciseId { get; set; }
        public int ExerciseScore { get; set; }
        public System.DateTime ExerciseDate { get; set; }
        public int currentUserLevel { get; set; }
        public int xpNeededForNext { get; set; }
        public int nextUserLevel { get; set; }
        public int totalXpForThisLevelEquals { get; set; }
        public int totalXpForNextLevel { get; set; }
        public int currentUserXp { get; set; }
        public int OldCurrentLevel { get; set; }
        public int OldNextLevel { get; set; }
        public int XpForCurrentLevelEquals { get; set; }
        public int XpForNextLevelEquals { get; set; }

    }
    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }
}