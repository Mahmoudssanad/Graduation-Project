
using System.Text.RegularExpressions;

namespace GreenEye.CustomValidation
{
    public class UserValidator : IUserValidator<ApplicationUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            var errors = new List<IdentityError>();

            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNameRequired",
                    Description = "UserName cannot be empty."
                }));
            }

            var regex = new Regex(@"^[\u0600-\u06FFa-zA-Z0-9 _.-]+$");

            if (!regex.IsMatch(user.UserName!))
            {
                errors.Add(new IdentityError
                {
                    Code = "Invalidusername",
                    Description = "username should be contain arabic, english char, numbers and some special char(._-)"
                });
            }

            if (errors.Any())
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            // FromResult => asynchronous او تشغيل await هي طريقه لتحويل اي قيمه عاديه من غير ما تعمل 
            // await استخدمتها لان رساله النجاح او الفشل هي مجرد انها هتشيلها بس بمعني ادق العمليه متزامنه ومش محتاجه 
            return Task.FromResult(IdentityResult.Success);
        }
    }
}
