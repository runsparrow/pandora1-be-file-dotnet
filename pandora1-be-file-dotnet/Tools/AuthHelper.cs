using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace pandora1_be_file_dotnet.Tools
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthHelper
    {

        /// <summary>
        /// 从Claim获取UserId
        /// </summary>
        /// <returns></returns>
        public static ClaimEntity GetClaimFromToken(string token = null)
        {
            // 定义
            ClaimEntity entity = new ClaimEntity();
            // Token校验
            if(token != null)
            {
                // 获取Claims
                IEnumerator<Claim> ienumerator = new JwtSecurityToken(token).Claims.GetEnumerator();
                // 遍历
                while (ienumerator.MoveNext())
                {
                    var claim = ienumerator.Current;
                    if (claim.Type.ToLower().Equals("id"))
                    {
                        entity.Id = int.Parse(claim.Value);
                    }
                    if (claim.Type.ToLower().Equals("name"))
                    {
                        entity.Name = claim.Value;
                    }
                    if (claim.Type.ToLower().Equals("realname"))
                    {
                        entity.RealName = claim.Value;
                    }
                    if (claim.Type.ToLower().Equals("nickname"))
                    {
                        entity.NickName = claim.Value;
                    }
                }
            }
            // 返回
            return entity;
        }
        /// <summary>
        /// 
        /// </summary>
        public class ClaimEntity
        {
            /// <summary>
            /// 
            /// </summary>
            public int Id { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string RealName { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string NickName { get; set; }
        }
    }
}
