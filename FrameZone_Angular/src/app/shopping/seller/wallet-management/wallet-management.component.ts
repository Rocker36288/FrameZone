import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface Transaction {
  id: string;
  type: 'income' | 'withdraw' | 'refund';
  amount: number;
  description: string;
  date: Date;
  status: 'completed' | 'pending' | 'failed';
}

@Component({
  selector: 'app-wallet-management',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './wallet-management.component.html',
  styleUrl: './wallet-management.component.css'
})
export class WalletManagementComponent {
  // 餘額資訊
  balance: number = 125680;
  pendingAmount: number = 32450;
  totalIncome: number = 458920;
  totalWithdraw: number = 301790;

  // 交易記錄
  transactions: Transaction[] = [];
  filteredTransactions: Transaction[] = [];
  currentFilter: 'all' | 'income' | 'withdraw' | 'refund' = 'all';
  searchQuery: string = '';

  ngOnInit(): void {
    this.loadMockData();
    this.applyFilters();
  }

  loadMockData(): void {
    this.transactions = [
      {
        id: '1',
        type: 'income',
        amount: 42900,
        description: '訂單收入 - ORD-2024-001 (iPhone 15 Pro Max)',
        date: new Date('2024-12-20 14:30'),
        status: 'completed'
      },
      {
        id: '2',
        type: 'withdraw',
        amount: 50000,
        description: '提領至銀行帳戶 (國泰世華 ****1234)',
        date: new Date('2024-12-19 10:00'),
        status: 'pending'
      },
      {
        id: '3',
        type: 'income',
        amount: 59900,
        description: '訂單收入 - ORD-2024-002 (MacBook Pro)',
        date: new Date('2024-12-18 16:20'),
        status: 'completed'
      },
      {
        id: '4',
        type: 'refund',
        amount: 7490,
        description: '訂單退款 - ORD-2024-015 (AirPods Pro)',
        date: new Date('2024-12-17 11:45'),
        status: 'completed'
      },
      {
        id: '5',
        type: 'income',
        amount: 18900,
        description: '訂單收入 - ORD-2024-003 (iPad Air)',
        date: new Date('2024-12-16 09:15'),
        status: 'completed'
      },
      {
        id: '6',
        type: 'withdraw',
        amount: 30000,
        description: '提領至銀行帳戶 (國泰世華 ****1234)',
        date: new Date('2024-12-15 14:00'),
        status: 'completed'
      },
      {
        id: '7',
        type: 'income',
        amount: 12900,
        description: '訂單收入 - ORD-2024-004 (Apple Watch)',
        date: new Date('2024-12-14 13:30'),
        status: 'completed'
      },
      {
        id: '8',
        type: 'withdraw',
        amount: 20000,
        description: '提領至銀行帳戶 (國泰世華 ****1234)',
        date: new Date('2024-12-13 10:30'),
        status: 'failed'
      }
    ];
  }

  applyFilters(): void {
    let result = [...this.transactions];

    // 類型篩選
    if (this.currentFilter !== 'all') {
      result = result.filter(t => t.type === this.currentFilter);
    }

    // 搜尋
    if (this.searchQuery.trim()) {
      const query = this.searchQuery.toLowerCase();
      result = result.filter(t =>
        t.description.toLowerCase().includes(query) ||
        t.amount.toString().includes(query)
      );
    }

    this.filteredTransactions = result;
  }

  onFilterChange(filter: 'all' | 'income' | 'withdraw' | 'refund'): void {
    this.currentFilter = filter;
    this.applyFilters();
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  requestWithdraw(): void {
    alert('申請提領功能\n可用餘額: NT$ ' + this.balance.toLocaleString());
  }

  getTypeText(type: string): string {
    const typeMap: { [key: string]: string } = {
      income: '收入',
      withdraw: '提領',
      refund: '退款'
    };
    return typeMap[type] || type;
  }

  getTypeClass(type: string): string {
    return `type-${type}`;
  }

  getStatusText(status: string): string {
    const statusMap: { [key: string]: string } = {
      completed: '已完成',
      pending: '處理中',
      failed: '失敗'
    };
    return statusMap[status] || status;
  }

  getStatusClass(status: string): string {
    return `status-${status}`;
  }
}
