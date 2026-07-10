import { ProductReadModel } from './product-read';

export interface CatalogPage {
  items: ProductReadModel[];
  nextCursor: number;
  hasMore: boolean;
}
