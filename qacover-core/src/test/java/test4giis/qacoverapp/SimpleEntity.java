package test4giis.qacoverapp;

/**
 * Used in test for Apache commons DbUtils
 */
public class SimpleEntity {
	private Integer id;
	private Integer num;
	private String text;
	public Integer getId() { return this.id; }
	public Integer getNum() { return this.num; }
	public String getText() { return this.text; }
	public void setId(Integer value) { this.id = value; }
	public void setNum(Integer value) { this.num = value; }
	public void setText(String value) { this.text = value; }
}
