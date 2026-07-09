export interface ProductReadModel {
  productId: string;
  name: string;
  description: string;
  price: number;
  sku: string;
  versionNumber: number;
  versionId: string;
  approvedBy: string;
  approvedAt: Date;
}
