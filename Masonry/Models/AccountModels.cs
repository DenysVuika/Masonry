/*
The MIT License (MIT)
Copyright (c) 2012 Denys Vuika

Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
and associated documentation files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.ComponentModel.DataAnnotations;
using Masonry.Resources;

namespace Masonry.Models
{

  public class ChangePasswordModel
  {
    [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorRequiredField")]
    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(Strings), Name = "CurrentPassword")]
    public string OldPassword { get; set; }

    [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorRequiredField")]
    [StringLength(100, MinimumLength = 6, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorStringLength")]
    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(Strings), Name = "NewPassword")]
    public string NewPassword { get; set; }

    [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "RequiredField")]
    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(Strings), Name = "ConfirmNewPassword")]
    [System.Web.Mvc.Compare("NewPassword",ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorComparePassword")]
    public string ConfirmPassword { get; set; }
  }

  public class LogOnModel
  {
    [Required]
    [Display(Name = "Account", Prompt = "i.e. johndoe")]
    [RegularExpression("^[a-zA-Z0-9_-]{3,40}$", ErrorMessage = "Invalid username format")]
    [StringLength(40, MinimumLength = 3, ErrorMessage = "User name must be between 3 and 40 characters long.")]
    public string Account { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
  }

  public class RegisterModel
  {
    [Required]
    [Display(Name = "Account", Prompt = "i.e. johndoe")]
    [RegularExpression("^[a-zA-Z0-9_-]{3,40}$", ErrorMessage = "Invalid username format")]
    [StringLength(40, MinimumLength = 3, ErrorMessage = "User name must be between 3 and 40 characters long.")]
    public string Account { get; set; }

    [Required]
    [Display(Name = "Name", Prompt = "i.e. John Doe")]
    public string Name { get; set; }

    [Required]
    [DataType(DataType.EmailAddress)]
    [Display(Name = "Email address", Prompt = "i.e. johndoe@mail.com")]
    [RegularExpression(@"^[a-zA-Z0-9.!#$%&amp;'*+-/=?\^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$", ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [System.Web.Mvc.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }
  }
}
