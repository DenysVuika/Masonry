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
    [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(Strings), Name = "Account", Prompt = "AccountPrompt")]
    [RegularExpression("^[a-zA-Z0-9_-]{3,40}$", ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorAccountFormat")]
    [StringLength(40, MinimumLength = 3, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorStringLength")]
    public string Account { get; set; }

    [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "RequiredField")]
    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(Strings), Name = "Password")]
    public string Password { get; set; }

    [Display(ResourceType = typeof(Strings), Name = "RememberMe")]
    public bool RememberMe { get; set; }
  }

  public class RegisterModel
  {
    [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(Strings), Name = "Account", Prompt = "AccountPrompt")]
    [RegularExpression("^[a-zA-Z0-9_-]{3,40}$", ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorAccountFormat")]
    [StringLength(40, MinimumLength = 3, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorStringLength")]
    public string Account { get; set; }

    [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(Strings), Name = "Name", Prompt = "NamePrompt")]
    public string Name { get; set; }

    [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "RequiredField")]
    [DataType(DataType.EmailAddress)]
    [Display(ResourceType = typeof(Strings), Name = "EmailAddress", Prompt = "EmailAddressPrompt")]
    [RegularExpression(@"^[a-zA-Z0-9.!#$%&amp;'*+-/=?\^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$", ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorEmailFormat")]
    public string Email { get; set; }

    [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "RequiredField")]
    [StringLength(100, MinimumLength = 6, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorStringLength")]
    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(Strings), Name = "Password")]
    public string Password { get; set; }

    [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "RequiredField")]
    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(Strings), Name = "ConfirmPassword")]
    [System.Web.Mvc.Compare("Password", ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorComparePassword")]
    public string ConfirmPassword { get; set; }
  }
}
