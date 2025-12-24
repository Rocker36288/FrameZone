import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

interface Category {
  id: string;
  name: string;
  order: number;
  isActive: boolean;
  productCount: number;
  productIds: string[];
}

interface Product {
  id: string;
  name: string;
  image: string;
  price: number;
  sku: string;
}

@Component({
  selector: 'app-category-settings',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink],
  templateUrl: './category-settings.component.html',
  styleUrl: './category-settings.component.css'
})
export class CategorySettingsComponent {
  categories: Category[] = [
    {
      id: '1',
      name: '服飾配件',
      order: 1,
      isActive: true,
      productCount: 3,
      productIds: ['1', '2', '3']
    },
    {
      id: '2',
      name: '3C產品',
      order: 2,
      isActive: true,
      productCount: 2,
      productIds: ['4', '5']
    },
    {
      id: '3',
      name: '居家生活',
      order: 3,
      isActive: true,
      productCount: 1,
      productIds: ['6']
    },
    {
      id: '4',
      name: '美妝保養',
      order: 4,
      isActive: false,
      productCount: 0,
      productIds: []
    }
  ];

  allProducts: Product[] = [
    { id: '1', name: '棉質T恤', image: 'https://via.placeholder.com/60', price: 590, sku: 'SKU001' },
    { id: '2', name: '牛仔褲', image: 'https://via.placeholder.com/60', price: 1290, sku: 'SKU002' },
    { id: '3', name: '運動背包', image: 'https://via.placeholder.com/60', price: 1590, sku: 'SKU003' },
    { id: '4', name: '無線耳機', image: 'https://via.placeholder.com/60', price: 2990, sku: 'SKU004' },
    { id: '5', name: '手機殼', image: 'https://via.placeholder.com/60', price: 390, sku: 'SKU005' },
    { id: '6', name: '香氛蠟燭', image: 'https://via.placeholder.com/60', price: 450, sku: 'SKU006' },
    { id: '7', name: '保濕面霜', image: 'https://via.placeholder.com/60', price: 890, sku: 'SKU007' },
    { id: '8', name: '藍芽音箱', image: 'https://via.placeholder.com/60', price: 1490, sku: 'SKU008' },
  ];

  showCategoryModal = false;
  showDeleteModal = false;
  currentCategory: Category | null = null;
  categoryToDelete: Category | null = null;
  isEditMode = false;
  searchKeyword: string = '';

  newCategory: Category = {
    id: '',
    name: '',
    order: 0,
    isActive: true,
    productCount: 0,
    productIds: []
  };

  ngOnInit(): void {
    // 初始化邏輯
  }

  // Category Management
  openAddModal(): void {
    this.isEditMode = false;
    this.searchKeyword = '';
    this.newCategory = {
      id: Date.now().toString(),
      name: '',
      order: this.categories.length + 1,
      isActive: true,
      productCount: 0,
      productIds: []
    };
    this.currentCategory = null;
    this.showCategoryModal = true;
  }

  openEditModal(category: Category): void {
    this.isEditMode = true;
    this.searchKeyword = '';
    // 深拷貝整個物件,包括 productIds 陣列
    this.currentCategory = {
      ...category,
      productIds: [...category.productIds]
    };
    this.newCategory = {
      id: Date.now().toString(),
      name: '',
      order: 0,
      isActive: true,
      productCount: 0,
      productIds: []
    };
    this.showCategoryModal = true;
  }

  get modalCategory(): Category {
    return this.currentCategory || this.newCategory;
  }

  closeModal(): void {
    this.showCategoryModal = false;
    this.currentCategory = null;
    this.searchKeyword = '';
  }

  saveCategory(): void {
    if (!this.modalCategory.name.trim()) {
      alert('請輸入分類名稱');
      return;
    }

    if (this.isEditMode && this.currentCategory) {
      // 更新商品數量
      this.currentCategory.productCount = this.currentCategory.productIds.length;

      // 找到要更新的分類
      const index = this.categories.findIndex(c => c.id === this.currentCategory!.id);
      if (index !== -1) {
        // 完整替換整個物件
        this.categories[index] = {
          id: this.currentCategory.id,
          name: this.currentCategory.name,
          order: this.currentCategory.order,
          isActive: this.currentCategory.isActive,
          productCount: this.currentCategory.productIds.length,
          productIds: [...this.currentCategory.productIds]
        };
      }
    } else {
      // 新增分類
      this.newCategory.productCount = this.newCategory.productIds.length;
      this.categories.push({
        ...this.newCategory,
        productIds: [...this.newCategory.productIds]
      });
    }

    console.log('儲存後的分類:', this.categories);
    this.closeModal();
  }

  confirmDelete(category: Category): void {
    this.categoryToDelete = category;
    this.showDeleteModal = true;
  }

  deleteCategory(): void {
    if (this.categoryToDelete) {
      this.categories = this.categories.filter(c => c.id !== this.categoryToDelete!.id);
      this.showDeleteModal = false;
      this.categoryToDelete = null;
    }
  }

  toggleCategory(category: Category): void {
    category.isActive = !category.isActive;
  }

  moveUp(category: Category): void {
    const index = this.categories.findIndex(c => c.id === category.id);
    if (index > 0) {
      [this.categories[index], this.categories[index - 1]] =
        [this.categories[index - 1], this.categories[index]];
      this.updateOrder();
    }
  }

  moveDown(category: Category): void {
    const index = this.categories.findIndex(c => c.id === category.id);
    if (index < this.categories.length - 1) {
      [this.categories[index], this.categories[index + 1]] =
        [this.categories[index + 1], this.categories[index]];
      this.updateOrder();
    }
  }

  private updateOrder(): void {
    this.categories.forEach((cat, index) => {
      cat.order = index + 1;
    });
  }

  // Product Selection
  get filteredProducts(): Product[] {
    if (!this.searchKeyword.trim()) {
      return this.allProducts;
    }
    const keyword = this.searchKeyword.toLowerCase();
    return this.allProducts.filter(p =>
      p.name.toLowerCase().includes(keyword) ||
      p.sku.toLowerCase().includes(keyword)
    );
  }

  isProductSelected(productId: string): boolean {
    return this.modalCategory.productIds.includes(productId);
  }

  toggleProduct(productId: string): void {
    const index = this.modalCategory.productIds.indexOf(productId);
    if (index > -1) {
      this.modalCategory.productIds.splice(index, 1);
    } else {
      this.modalCategory.productIds.push(productId);
    }
  }

  getSelectedProductsCount(): number {
    return this.modalCategory.productIds.length;
  }
}
