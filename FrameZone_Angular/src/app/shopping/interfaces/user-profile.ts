/**
 * 使用者資料 DTO (對應後端 User, UserProfile, UserPrivateInfo)
 */
export interface ShoppingUserProfile {
    account: string;        // User.Account
    email: string;          // User.Email
    phone: string;          // User.Phone
    displayName: string;    // UserProfile.DisplayName
    avatar: string;         // UserProfile.Avatar
    realName: string;       // UserPrivateInfo.RealName
    gender: string;         // UserPrivateInfo.Gender
    birthDate: string;      // UserPrivateInfo.BirthDate
}
