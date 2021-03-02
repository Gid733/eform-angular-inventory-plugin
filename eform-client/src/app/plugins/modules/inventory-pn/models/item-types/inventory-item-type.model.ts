export class InventoryItemTypeModel {
  id: number;
  parentId: number | null;
  parent: InventoryItemTypeModel;
  name: string;
  description: string;
  code: string;
}
