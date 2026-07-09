import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CategoryDemo } from './category-demo';

describe('CategoryDemo', () => {
  let component: CategoryDemo;
  let fixture: ComponentFixture<CategoryDemo>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CategoryDemo],
    }).compileComponents();

    fixture = TestBed.createComponent(CategoryDemo);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
