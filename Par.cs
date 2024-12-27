namespace Par;

public class Pair  {
    int x, y;

    public Pair(int _x, int _y)  {
        x = _x;
        y = _y;
    }

    public static bool igual(Pair A, Pair B)  {
        if(A.x == B.x && A.y == B.y)
          return true;
        return false;
    }

    public int GetX() {
        return x;
    }
    public int GetY() {
        return y;
    }

    public void setX(int x1)  {
        x = x1;
    }  

    public void setY(int y1)  {
        y = y1;
    }

}