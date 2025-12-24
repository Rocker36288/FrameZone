import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface BankAccount {
  id: string;
  bankName: string;
  bankCode: string;
  accountNumber: string;
  accountName: string;
  isDefault: boolean;
  verified: boolean;
  createdAt: Date;
}

@Component({
  selector: 'app-bank-account-management',
  imports: [FormsModule, CommonModule],
  templateUrl: './bank-account-management.component.html',
  styleUrl: './bank-account-management.component.css'
})
export class BankAccountManagementComponent {
  accounts: BankAccount[] = [
    {
      id: '1',
      bankName: '國泰世華銀行',
      bankCode: '013',
      accountNumber: '1234567890123',
      accountName: '王大明',
      isDefault: true,
      verified: true,
      createdAt: new Date('2024-01-15')
    },
    {
      id: '2',
      bankName: '台北富邦銀行',
      bankCode: '012',
      accountNumber: '9876543210987',
      accountName: '王大明',
      isDefault: false,
      verified: true,
      createdAt: new Date('2024-03-20')
    },
    {
      id: '3',
      bankName: '中國信託銀行',
      bankCode: '822',
      accountNumber: '5555666677778',
      accountName: '王大明',
      isDefault: false,
      verified: false,
      createdAt: new Date('2024-12-01')
    }
  ];

  showAddForm: boolean = false;

  newAccount: BankAccount = {
    id: '',
    bankName: '',
    bankCode: '',
    accountNumber: '',
    accountName: '',
    isDefault: false,
    verified: false,
    createdAt: new Date()
  };

  addAccount(): void {
    this.showAddForm = true;
  }

  cancelAdd(): void {
    this.showAddForm = false;
    this.resetForm();
  }

  saveAccount(): void {
    if (!this.newAccount.bankName || !this.newAccount.accountNumber || !this.newAccount.accountName) {
      alert('請填寫完整資料');
      return;
    }

    const account: BankAccount = {
      ...this.newAccount,
      id: Date.now().toString(),
      verified: false,
      createdAt: new Date()
    };

    this.accounts.push(account);
    this.showAddForm = false;
    this.resetForm();
    alert('銀行帳號已新增，等待審核中');
  }

  resetForm(): void {
    this.newAccount = {
      id: '',
      bankName: '',
      bankCode: '',
      accountNumber: '',
      accountName: '',
      isDefault: false,
      verified: false,
      createdAt: new Date()
    };
  }

  setDefault(account: BankAccount): void {
    if (!account.verified) {
      alert('未驗證的帳號無法設為預設');
      return;
    }

    this.accounts.forEach(acc => acc.isDefault = false);
    account.isDefault = true;
    alert(`已將 ${account.bankName} 帳號設為預設提領帳號`);
  }

  deleteAccount(account: BankAccount): void {
    if (account.isDefault) {
      alert('無法刪除預設帳號，請先設定其他帳號為預設');
      return;
    }

    if (confirm(`確定要刪除 ${account.bankName} 帳號嗎？`)) {
      this.accounts = this.accounts.filter(acc => acc.id !== account.id);
      alert('帳號已刪除');
    }
  }

  maskAccountNumber(accountNumber: string): string {
    if (accountNumber.length <= 4) return accountNumber;
    return accountNumber.slice(0, 3) + '****' + accountNumber.slice(-4);
  }
}
