import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
  searchControl = new FormControl('');
  
  @Output() search = new EventEmitter<string>();
  @Output() reset = new EventEmitter<void>();

  clearSearch(): void {
    this.searchControl.setValue('');
  }

  onSearch(): void {
    const value = this.searchControl.value || '';
    this.search.emit(value);
  }

  resetFilters(): void {
    this.searchControl.setValue('');
    this.reset.emit();
  }
}
