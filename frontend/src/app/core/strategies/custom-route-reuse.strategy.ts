import { RouteReuseStrategy, ActivatedRouteSnapshot, DetachedRouteHandle } from '@angular/router';

export class CustomRouteReuseStrategy implements RouteReuseStrategy {
  private static handlers: { [key: string]: DetachedRouteHandle } = {};

  public static clearCache(): void {
    CustomRouteReuseStrategy.handlers = {};
  }

  public shouldDetach(route: ActivatedRouteSnapshot): boolean {
    const path = route.routeConfig?.path || '';
    if (path === 'login' || path === 'register' || path === 'forgot-password' || path === 'reset-password') {
      return false;
    }
    return !!route.routeConfig;
  }

  public store(route: ActivatedRouteSnapshot, handle: DetachedRouteHandle | null): void {
    if (route.routeConfig) {
      const key = this.getRouteKey(route);
      if (handle) {
        CustomRouteReuseStrategy.handlers[key] = handle;
      } else {
        delete CustomRouteReuseStrategy.handlers[key];
      }
    }
  }

  public shouldAttach(route: ActivatedRouteSnapshot): boolean {
    if (!route.routeConfig) return false;
    const key = this.getRouteKey(route);
    return !!CustomRouteReuseStrategy.handlers[key];
  }

  public retrieve(route: ActivatedRouteSnapshot): DetachedRouteHandle | null {
    if (!route.routeConfig) return null;
    const key = this.getRouteKey(route);
    return CustomRouteReuseStrategy.handlers[key] || null;
  }

  public shouldReuseRoute(future: ActivatedRouteSnapshot, curr: ActivatedRouteSnapshot): boolean {
    return future.routeConfig === curr.routeConfig;
  }

  private getRouteKey(route: ActivatedRouteSnapshot): string {
    let path = route.routeConfig?.path || '';
    let current = route;
    while (current.parent) {
      current = current.parent;
      if (current.routeConfig?.path) {
        path = current.routeConfig.path + '/' + path;
      }
    }
    return `${path}?${JSON.stringify(route.params)}`;
  }
}
